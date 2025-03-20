# DOTNET-LLM-RAG

## Overview
This is a .NET console application that implements Retrieval-Augmented Generation (RAG) using **LocalAI** for enhanced AI responses. The system retrieves relevant text from a local knowledge base, augments the query with context, and generates a response using an LLM.

## Features
- 🧠 **Vector-Based Search** – Uses cosine similarity to find the most relevant text  
- 🤖 **LocalAI Integration** – Calls LocalAI API for embeddings and response generation  
- ⚡ **Fast & Lightweight** – Runs locally without cloud dependencies  

## Tech Stack
- **Language:** C# (.NET 7+)  
- **AI Model:** Tested(`deepseek-r1:1.5b`, `avr/sfr-embedding-mistral`)  
- **Vector Search:** Cosine similarity  
- **Storage:** In-memory dictionary (can be extended to use vector databases)  

## Prerequisites
- .NET 8 or later installed
- OLLAMA installed
- TXT file with prepared data

## InstalLation
# 1 git clone https://github.com/2kuba1/DOTNET-LLM-RAG.git
# 2 cd path/to/your/project
# 3 dotnet build
# 4 dotnet run

**TXT FILE IS REQUIRED AND SHOULD BE STORED AT IN SOLUTION FOLDER** <br />
**JSON FILE IS REQUIRED AND SHOULD BE STORED AT IN SOLUTION FOLDER TO USE REMEMBERING**

- const string uri = "";
- const string embeddingModel = "";
- const string respondingModel = "";
- const string txtFilePath = "";
- const string vectorStoreFilePath = ""; - fill with .json file 

Should be filled with your data
