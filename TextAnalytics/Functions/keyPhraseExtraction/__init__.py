import logging
import azure.functions as func
from . import extract_keyPhrases as aze
import json


def main(inputblob: func.InputStream, doc: func.Out[func.Document], outputblob: func.Out[str]):
    request_json = json.load(inputblob)
    text = request_json["Text"]

    logging.info(f"Processing request from blob {inputblob.name}")
    
    response_doc = aze.extract_keyPhrases([text], inputblob.name)
    outputblob.set(json.dumps(response_doc))

    doclist = func.DocumentList()
    doclist.append(func.Document.from_dict(response_doc))
    doc.set(doclist)
    
    logging.info(f"keyPhraseExtraction was executed successfully.")
