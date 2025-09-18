from flask import Flask, request, jsonify
from FlagEmbedding import BGEM3FlagModel
import numpy as np

app = Flask(__name__)
# Load the model once when the service starts
# Setting use_fp16 to True can speed up computation with a slight performance degradation.
model = BGEM3FlagModel('BAAI/bge-m3', use_fp16=True) # [5]

@app.route('/embed', methods=['POST'])
def embed_text():
    data = request.json
    sentences = data.get('sentences', [])
    if not sentences:
        return jsonify({"error": "No sentences provided"}), 400

    # Generate both dense and sparse embeddings.
    # return_colbert_vecs=False unless you explicitly need and handle multi-vector representations.
    output = model.encode(sentences, return_dense=True, return_sparse=True, return_colbert_vecs=False) # [5]

    # Format sparse_vecs for JSON, as they are typically numpy arrays or dicts
    formatted_sparse_vecs = []
    for item in output['sparse_vecs']:
        formatted_sparse_vecs.append({
            "indices": item['indices'].tolist(), # Convert numpy array to list
            "values": item['values'].tolist()   # Convert numpy array to list
        })

    return jsonify({
        "dense_embeddings": output['dense_vecs'].tolist(),
        "sparse_embeddings": formatted_sparse_vecs
    })

if __name__ == '__main__':
    # Run on a different port than Ollama (e.g., 5001)
    app.run(host='0.0.0.0', port=5555)