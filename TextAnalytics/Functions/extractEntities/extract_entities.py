import json
from ..shared_code import api_helper as te


def extract_entities(text: list, source: str) -> dict: 
    client = te.authenticate_client()
    response = client.recognize_entities(documents = text)[0]

    named_entity_response = {
        "document": text,
        "source": source,
        "entities": []
    }

    for idx, entity in enumerate(response.entities):
        named_entity = {
                "text": entity.text,
                "category": entity.category,
                "subCategory": entity.subcategory,
                "confidenceScore": round(entity.confidence_score, 2)
            }
        named_entity_response["entities"].append(named_entity)

    return named_entity_response 

# For testing only
if __name__ == "__main__":
    import sys

    print(json.dumps(extract_entities([sys.argv[1]], "test"), indent=4))
