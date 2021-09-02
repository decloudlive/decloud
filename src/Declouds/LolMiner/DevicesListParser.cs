﻿using DCL.Common;
using DCL.Common.Device;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LolDecloud
{
    internal class DevicesListParser
    {
        private static string[] _keywords = new string[] { "Device", "Address:" };

        private static bool KeepLine(string line)
        {
            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line)) return false;
            var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(_keywords.Contains);
        }
        

        private static int? NumberAfterPattern(string pattern, string line)
        {
            try
            {
                var index = line?.IndexOf(pattern) ?? -1;
                if (index < 0) return null;
                var numericChars = line
                    .Substring(index + pattern.Length)
                    .SkipWhile(c => !char.IsDigit(c))
                    .TakeWhile(char.IsDigit)
                    .ToArray();
                var numberStr = new string(numericChars);
                if (int.TryParse(numberStr, out var number)) return number;
            }
            catch
            { }
            return null;
        }

        private static int[] ChunkToGPU_PCIe_Pair(string[] chunk)
        {
            return _keywords.Zip(chunk, (pattern, line) => (pattern, line))
                .Select(p => NumberAfterPattern(p.pattern, p.line))
                .Where(num => num.HasValue)
                .Select(num => num.Value)
                .ToArray();
        }

        public static IEnumerable<(string uuid, int DecloudGpuId)> ParseLolDecloudOutput(string output, IEnumerable<BaseDevice> baseDevices)
        {
            try
            {
                var gpus = baseDevices
                .Where(dev => dev is IGpuDevice)
                .Cast<IGpuDevice>()
                .ToArray();

                var mappedDevices = output.Split(new[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(KeepLine)
                    .Select((line, index) => (line, index))
                    .GroupBy(p => p.index / _keywords.Length)
                    .Select(g => g.Select(p => p.line).ToArray())
                    .Select(ChunkToGPU_PCIe_Pair)
                    .Where(nums => nums.Length == 2)
                    .Select(nums => (DecloudGpuId: nums[0], pcie: nums[1]))
                    .Select(p => (gpu: gpus.FirstOrDefault(gpu => gpu.PCIeBusID == p.pcie), p.DecloudGpuId))
                    .Where(p => p.gpu != null)
                    .Select(p => (uuid: p.gpu.UUID, p.DecloudGpuId))
                    .ToArray();
                return mappedDevices;
            }
            catch (Exception e)
            {
                Logger.Error("LolDecloudPlugin", $"DevicesListParser error: {e.Message}");
                return Enumerable.Empty<(string uuid, int DecloudGpuId)>();
            }
        }
    }
}
