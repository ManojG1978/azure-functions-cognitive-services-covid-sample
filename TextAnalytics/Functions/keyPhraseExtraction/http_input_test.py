import logging
import azure.functions as func
from . import extract_keyPhrases as aze
import json

def main(req: func.HttpRequest, outputBlob: func.Out[str]) -> func.HttpResponse:

    logging.info(f"Processing request from Http Request")
    text = req.params.get('text')
    if not text:
        try:
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            text = req_body.get('text')

    if text:
        outputBlob.set(aze.extract_keyPhrases([text]))

        return func.HttpResponse(f"keyPhraseExtraction was executed successfully.")
    else:
        return func.HttpResponse(
            "Pass a text in the query string or in the request body for extracting the key phrases",
            status_code=400
        )

