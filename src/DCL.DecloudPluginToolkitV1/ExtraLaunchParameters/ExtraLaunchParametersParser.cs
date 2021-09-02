using DCL.Common;
using DCL.DecloudPlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCL.DecloudPluginToolkitV1.ExtraLaunchParameters
{
    /// <summary>
    /// This class is used to Parse Extra Launch Parameters
    /// </summary>
    public static class ExtraLaunchParametersParser
    {
        /// <summary>
        /// ParsedDecloudOption is used to set parsed value to DecloudOption <see cref="DecloudOption"/>
        /// </summary>
        private class ParsedDecloudOption
        {
            public DecloudOption Option { get; set; }
            public string Value { get; set; } = null;
        }

        /// <summary>
        /// ParsedDeviceDecloudOptions is used to parse all DecloudOptions and parameters for each device
        /// </summary>
        private class ParsedDeviceDecloudOptions
        {
            public string DeviceUUID { get; set; }
            public List<string> Parameters { get; set; }
            public List<ParsedDecloudOption> ParsedDecloudOptions { get; set; }
        }

        /// <summary>
        /// MergedParsedDecloudOption is used to merge parameters and values for devices. They are merged in device order.
        /// </summary>
        private class MergedParsedDecloudOption
        {
            public DecloudOption Option { get; set; }
            public List<string> Values { get; set; } = new List<string>();
            public bool IsDefaults { get; set; }
        }

        /// <summary>
        /// IsOptionDefaultValue checks if parsed value is same as default value of DecloudOption
        /// </summary>
        private static bool IsOptionDefaultValue(DecloudOption option, string value)
        {
            // Check if value is null or empty
            if (string.IsNullOrEmpty(value)) return true;
            if (string.IsNullOrWhiteSpace(value)) return true;

            // Value is not null or empty, compare it with default value
            if (!value.Equals(option.DefaultValue)) return false;

            return true;
        }

        public static bool CheckIfCanGroup(MiningPair a, MiningPair b, List<DecloudOption> options, bool ignoreDefaultValueOptions)
        {
            if (options == null || options.Count == 0) return true;

            var filteredOptionsSingle = options.Where(optionType => optionType.Type == DecloudOptionType.OptionWithSingleParameter).ToList();
            var filteredOptionsIsParam = options.Where(optionType => optionType.Type == DecloudOptionType.OptionIsParameter).ToList();

            var parsedASingle = Parse(new List<MiningPair> { a }, filteredOptionsSingle, ignoreDefaultValueOptions);
            var parsedBSingle = Parse(new List<MiningPair> { b }, filteredOptionsSingle, ignoreDefaultValueOptions);

            if (parsedASingle != parsedBSingle) return false;

            var parsedAParam = Parse(new List<MiningPair> { a }, filteredOptionsIsParam, ignoreDefaultValueOptions);
            var parsedBParam = Parse(new List<MiningPair> { b }, filteredOptionsIsParam, ignoreDefaultValueOptions);

            return parsedAParam == parsedBParam;
        }

        /// <summary>
        /// Main Parse function which gets List of Mining Pairs <see cref="MiningPair"/>, List of Decloud Options <see cref="DecloudOption"/> and UseIfDefaults (bool) as arguments
        /// IgnoreDefaultValueOptions argument is set to false if you would like to parse default values for extra launch parameters
        /// It returns parsed string ready for adding to Decloud command
        /// </summary>
        public static string Parse(List<MiningPair> miningPairs, List<DecloudOption> options, bool ignoreDefaultValueOptions = true)
        {
            if (options == null || options.Count == 0) return "";

            // init all devices and options before we check parameters
            // order of devices matters!
            // order of parameters may matter
            var devicesOptions = new List<ParsedDeviceDecloudOptions>();
            foreach (var miningPair in miningPairs)
            {
                var device = miningPair.Device;
                var algorithm = miningPair.Algorithm;
                var parameters = algorithm.ExtraLaunchParameters
                    .Replace("=", "= ")
                    .Split(' ')
                    .Where(s => !string.IsNullOrEmpty(s) && !string.IsNullOrWhiteSpace(s))
                    .ToList();

                var parsedDecloudOptions = options
                    .Select(opt => new ParsedDecloudOption { Option = opt, Value = null })
                    .ToList();

                var deviceOptions = new ParsedDeviceDecloudOptions
                {
                    DeviceUUID = device.UUID,
                    Parameters = parameters,
                    ParsedDecloudOptions = parsedDecloudOptions
                };
                devicesOptions.Add(deviceOptions);
            }

            // check parameters and values
            foreach (var deviceOptions in devicesOptions)
            {
                var parameters = deviceOptions.Parameters;

                //check for single/multi parameter cases
                for (int paramIndex = 0; paramIndex < parameters.Count; paramIndex++)
                {
                    var param = parameters[paramIndex];
                    string value = (paramIndex + 1) < parameters.Count ? parameters[paramIndex + 1] : "";

                    // find an option with the flag
                    var deviceParsedOption = deviceOptions.ParsedDecloudOptions
                        .Where(pOpt => param.Equals(pOpt.Option.ShortName) || param.Equals(pOpt.Option.LongName))
                        .FirstOrDefault();

                    if (deviceParsedOption == null) continue;

                    switch (deviceParsedOption.Option.Type)
                    {
                        case DecloudOptionType.OptionIsParameter:
                            deviceParsedOption.Value = param;
                            break;
                        case DecloudOptionType.OptionWithSingleParameter:
                        case DecloudOptionType.OptionWithMultipleParameters:
                        case DecloudOptionType.OptionWithDuplicateMultipleParameters:
                            deviceParsedOption.Value = value;
                            break;
                    }
                }
            }

            // merge parameters and values for devices
            // they are merged in device order
            var mergedParsedDecloudOptions = options
                .Select(option => new MergedParsedDecloudOption { Option = option })
                .ToArray();

            foreach (var deviceOptions in devicesOptions)
            {
                for (int optIndex = 0; optIndex < mergedParsedDecloudOptions.Length; optIndex++)
                {
                    var mergedOption = mergedParsedDecloudOptions[optIndex];
                    var parsedOption = deviceOptions.ParsedDecloudOptions[optIndex];
                    if (parsedOption.Option != mergedOption.Option)
                    {
                        throw new Exception("Options missmatch");
                    }
                    mergedOption.Values.Add(parsedOption.Value);
                }
            }

            // check if is all defaults
            foreach (var mergedParsedDecloudOption in mergedParsedDecloudOptions)
            {
                var option = mergedParsedDecloudOption.Option;
                var values = mergedParsedDecloudOption.Values;
                var isDefault = values.All(value => IsOptionDefaultValue(option, value));
                mergedParsedDecloudOption.IsDefaults = isDefault;
            }
            var isAllDefault = mergedParsedDecloudOptions.All(mpmopt => mpmopt.IsDefaults);

            // we don't parse if we have everything default and don't force defaults
            if (isAllDefault && ignoreDefaultValueOptions) return "";

            var retVal = "";
            foreach (var mergedParsedDecloudOption in mergedParsedDecloudOptions)
            {
                var option = mergedParsedDecloudOption.Option;
                var values = mergedParsedDecloudOption.Values;

                var optionName = string.IsNullOrEmpty(option.ShortName) ? option.LongName : option.ShortName;

                if (mergedParsedDecloudOption.IsDefaults && ignoreDefaultValueOptions) continue;
                // if options all default ignore
                switch (option.Type)
                {
                    case DecloudOptionType.OptionIsParameter:
                        retVal += $" {optionName}";
                        break;
                    case DecloudOptionType.OptionWithSingleParameter:
                        {
                            // get the first non default value
                            var firstNonDefaultValue = values
                                .Where(value => !IsOptionDefaultValue(option, value))
                                .FirstOrDefault();

                            var setValue = option.DefaultValue;
                            if (firstNonDefaultValue != null)
                            {
                                setValue = firstNonDefaultValue;
                            }
                            var mask = " {0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, optionName, setValue);
                            break;
                        }
                    case DecloudOptionType.OptionWithMultipleParameters:
                        {
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue);
                            var mask = " {0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = " {0}{1}";
                            }
                            retVal += string.Format(mask, optionName, string.Join(option.Delimiter, setValues));
                            break;
                        }
                    case DecloudOptionType.OptionWithDuplicateMultipleParameters:
                        {
                            var mask = "{0} {1}";
                            if (optionName.Contains("="))
                            {
                                mask = "{0}{1}";
                            }
                            var setValues = values
                                .Select(value => value != null ? value : option.DefaultValue)
                                .Select(value => string.Format(mask, optionName, value));
                            retVal += " " + string.Join(" ", setValues);
                            break;
                        }
                }
            }
            Logger.Debug("ELPParser", $"Successfully parsed {retVal} elp");
            return retVal;
        }
    }
}
