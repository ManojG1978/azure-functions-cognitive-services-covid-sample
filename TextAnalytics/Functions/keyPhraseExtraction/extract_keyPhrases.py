import json
from ..shared_code import api_helper as te


def extract_keyPhrases(text: list, source: str) -> dict:
    client = te.authenticate_client()
    response = client.extract_key_phrases(documents = text)[0]

    key_phrases_response = {
        "document": text,
        "source": source,
        "keyPhrases": []
    }

    [key_phrases_response["keyPhrases"].append(phrase) for phrase in response.key_phrases]
    
    return key_phrases_response 

# For testing only
if __name__ == "__main__":
    import sys

    print(json.dumps(extract_keyPhrases([sys.argv[1]], "test"), indent=4))
