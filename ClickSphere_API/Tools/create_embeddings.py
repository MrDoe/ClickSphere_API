import os
import requests
import base64
import json

def get_md_files(directory):
    md_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith(".md"):
                md_files.append(os.path.join(root, file))
    return md_files

def read_file_content(file_path):
    with open(file_path, 'r', encoding='utf-8') as file:
        return file.read()

def encode_content(content):
    return base64.b64encode(content.encode()).decode()

def post_to_api(file_path, content):
    url = "http://localhost:5007/storeRagEmbedding"
    headers = {
        "accept": "text/plain",
        "Authorization": "Bearer CfDJ8JeXlA-bkr5Mn7c-5KUlFdhrDqSgC6-8b4gObM3b6W38WK7p0gdm2q22sKburieFsJ9CDLyZ7d6wO3xyX6W-GnhZJweQV4EDX4HH2jBFLX9VlnzxG1VB8N8tbAm4lXk4kV8Be-zKwMjquHM2ZfNWXLwMKWAYiB5XFZyNk7HVzWprdygAZnaAMVTz9SZx7px6cJEi_nzeH48SzZYL_bJQdjKYtHaud0uGPnotMGrYInWBuPhaJW9Fr0LT6ehBbaFVWckjyLSGy7dB09hieRv1zIRwvmwbdsdGMivF5wsmqf_9",
        "Content-Type": "application/json"
    }
    data = {
        "filename": file_path,
        "content": encode_content(content)
    }

    response = requests.post(url, headers=headers, data=json.dumps(data), proxies={"http": None, "https": None})
    return response.text

directory = "C:\\Daten\\Entwicklung\\clickhouse-docs\\docs\\en"
md_files = get_md_files(directory)

for md_file in md_files:
    content = read_file_content(md_file)
    
    # get relative file path
    relFilePath = md_file[len(directory)+1:]

    print(f"Processing {relFilePath}...") 
    response = post_to_api(relFilePath, content)
    print(f"Response for {md_file}: {response}")
