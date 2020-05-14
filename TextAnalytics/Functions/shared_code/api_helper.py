import os
from azure.core.credentials import AzureKeyCredential
from azure.ai.textanalytics import TextAnalyticsClient

def authenticate_client():
    ta_credential = AzureKeyCredential(os.environ["textAnalyticsApiKey"])
    text_analytics_client = TextAnalyticsClient(
        endpoint=os.environ["textAnalyticsApiEndpoint"], credential=ta_credential)

    return text_analytics_client
