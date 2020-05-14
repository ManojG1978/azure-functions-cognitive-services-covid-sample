import json
from ..shared_code import api_helper as te

def analyse_sentiment(text: list, source: str) -> dict:
    client = te.authenticate_client()    
    response = client.analyze_sentiment(documents = text)[0]

    sentiment_response = {
        "document": text,
        "source": source,
        "documentSentiment" : response.sentiment,
        "overallScore": {
            "positive": response.confidence_scores.positive,
            "negative":  response.confidence_scores.negative,
            "neutral":  response.confidence_scores.neutral
        },
        "sentences": []
    }

    for idx, sentence in enumerate(response.sentences):
        sentence_sentiment = {
            "sentenceSentiment" : sentence.sentiment,
            "score": {
                "positive": sentence.confidence_scores.positive,
                "negative": sentence.confidence_scores.negative,
                "neutral":  sentence.confidence_scores.neutral
            },
        }
        sentiment_response["sentences"].append(sentence_sentiment)

    return sentiment_response 
# For testing only
if __name__ == "__main__":
    import sys

    print(json.dumps(analyse_sentiment([sys.argv[1]], "test"), indent=4))
