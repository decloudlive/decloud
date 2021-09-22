#!/usr/bin/python

import requests as rq
import json
import pprint
import sys

KEY_DIR = '/Users/fanz/Desktop/ias-keys'
# cert=(KEY_DIR + '/key.pem', KEY_DIR + '/cert.pem')
cert=KEY_DIR + '/all.pem'

print cert

def print_headers(headers):
    print 'HEADERS: '
    for k, v in headers.iteritems():
        print k, ': ', v

def parse_request(r):
    print '-------------BEGIN RESPONSE----------------------'
    print 'STATUS CODE: ', r.status_code
    print_headers(r.headers)

    try:
        j = r.json()
        print 'JSON: ', json.dumps(j, indent=4, separators=(',', ': '))
    except ValueError:
        pass
    print '--------------END OF RESPONSE--------------------'

intel_url = "https://as.sgx.trustedservices.intel.com:443/attestation/sgx/v1"

import sys

att_evidence_payload = {
    'isvEnclaveQuote': sys.argv[1],
}

r = rq.post(intel_url + '/report', json=att_evidence_payload, cert=cert)

parse_request(r)
print r.content
