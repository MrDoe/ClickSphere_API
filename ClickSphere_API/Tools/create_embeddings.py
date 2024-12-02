import os
import requests
import base64
import json
import re

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
        "Authorization": "Bearer CfDJ8JeXlA-bkr5Mn7c-5KUlFdikuxjPcODpmSty28z5xmJeup3WK52Nz_bF62TyRN4WzETq0UGojFJ9dAsD3okm7F3IHQnxY9gCXl0_3sFvHD5D_bpz5dHP0jQjeL26FDIWRe8pNsbsyIswoWOT8DvNndEiW4cxa8azj2PcynAQvPNtCsDiGFZmNM2e0d4pY4YG_lvZFdreFN-VOI0p2jHCGo_KU8CzJR-DIKoHkkkrpsTATFvIrOXd1-_Z4NeJuUlVEk32cJqVY1L63mkIC9CIkxheY0nNUR1kCjxSsL1lKy6c",
        "Content-Type": "application/json"
    }
    data = {
        "filename": file_path,
        "content": encode_content(content)
    }

    response = requests.post(url, headers=headers, data=json.dumps(data), proxies={"http": None, "https": None})
    return response.text

def chunk_markdown(content, max_tokens=1000):
    # Split content into paragraphs
    paragraphs = re.split(r'\n\s*\n', content)
    
    chunks = []
    current_chunk = []
    current_tokens = 0
    
    for paragraph in paragraphs:
        paragraph_tokens = len(paragraph.split())
        
        if current_tokens + paragraph_tokens > max_tokens:
            chunks.append('\n\n'.join(current_chunk))
            current_chunk = []
            current_tokens = 0
        
        current_chunk.append(paragraph)
        current_tokens += paragraph_tokens
    
    if current_chunk:
        chunks.append('\n\n'.join(current_chunk))
    
    return chunks

directory = "C:\\Daten\\Entwicklung\\ClickHouse\\docs\\en\\sql-reference"
md_files = get_md_files(directory)

for md_file in md_files:
    # get relative file path
    relFilePath = md_file[len(directory)+1:]

    # read file content
    content = read_file_content(md_file)

    # chunk content
    chunks = chunk_markdown(content)

    # post each chunk to the API
    for i, chunk in enumerate(chunks):
        print(f"Processing {relFilePath} - Chunk {i+1}/{len(chunks)}")
        post_to_api(relFilePath, chunk)
    
    print(f"Finished processing {relFilePath}")

print("Finished processing all files.")
