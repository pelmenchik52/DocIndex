# DocIndex

A Windows desktop application for **indexing text documents, clustering them, finding similar documents and measuring word importance (TF-IDF)**. Built with **C# / WPF** on **.NET 8** using the **MVVM** pattern.

> Originally developed as a university coursework project (Lviv Polytechnic National University).

<!-- Add a screenshot: put the image in docs/ and uncomment the line below -->
<!-- ![DocIndex main window](docs/screenshot-main.png) -->

---

## Table of contents

- [Features](#features)
- [How it works](#how-it-works)
- [Getting started](#getting-started)
- [User guide](#user-guide)
- [Architecture](#architecture)
- [Project structure](#project-structure)
- [Tech stack](#tech-stack)
- [Known limitations](#known-limitations)
- [Author](#author)

---

## Features

- **Import documents**: load individual `.txt` files or a whole folder of `.txt` files.
- **Indexing**: build an inverted index of the corpus and compute a **TF-IDF** weight for every word.
- **Keywords table**: view each word, its TF-IDF score and its frequency across all documents. The table can be exported.
- **Similar documents**: select a document and find the documents most similar to it (cosine similarity).
- **Clustering**: group documents into *k* clusters with **K-Means**, where *k* is chosen with a slider.
- **Frequency dictionary**: a separate window with every word and how many times it occurs.
- **Statistics**: summary statistics about the loaded corpus.
- **Save / load corpus**: store the corpus in a JSON file and reopen it later.
- **Localization**: the interface supports multiple languages through `.resx` resources.
- **Themes**: switchable visual themes.
- **Responsive UI**: heavy operations (indexing, similarity search, clustering, file import) run in the background, so the window never freezes. A progress indicator and a log panel show what is happening.

---

## How it works

1. **Import.** Each text file becomes a `Document` and is added to the `Corpus`.
2. **Indexing.** The indexer tokenizes every document and builds an inverted index. For each word it computes:
   - **TF** (term frequency): how often the word appears in a document;
   - **IDF** (inverse document frequency): how rare the word is across the corpus;
   - **TF-IDF**: `TF × IDF`, a measure of how important the word is for a given document.
3. **Similarity search.** Documents are represented as TF-IDF vectors. The similarity between the selected document and every other one is computed with **cosine similarity**, and results above a threshold are shown (the default threshold is `0.1`).
4. **Clustering.** Documents are grouped by the **K-Means** algorithm (up to 100 iterations). The number of clusters *k* cannot exceed the number of documents.

---

## Getting started

### Requirements

- Windows 10 or 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (with the **.NET desktop development** workload), or the `dotnet` CLI

### Run from Visual Studio

1. Clone the repository:
   ```bash
   git clone https://github.com/pelmenchik52/DocIndex.git
   cd DocIndex
   ```
2. Open `DocIndex.sln` in Visual Studio.
3. Set **DocIndex** as the startup project and press **F5**.

### Run from the command line

```bash
dotnet restore
dotnet build
dotnet run
```

> WPF applications run on Windows only.

---

## User guide

> **Important.** The **Clustering**, **Find similar** and **Frequency dictionary** buttons are disabled until you press **Build index**. **Find similar** also requires a selected document. Changing the corpus (adding or removing documents) resets the index, so build it again.

### Typical workflow

1. Import documents (**Import files** or **Import folder**).
2. Press **Build index**.
3. Explore the results in the tabs, or use the analysis buttons.

### Tabs

| Tab | What it shows |
|---|---|
| **Documents** | A list of all loaded documents. |
| **Similar** | Documents similar to the selected one. |
| **Clusters** | Documents grouped into clusters. The number of clusters is set by the user. |
| **Keywords** | A table with each word, its TF-IDF score and its frequency across all documents. The table can be exported with the **Export** button. |
| **Statistics** | Basic statistics about the corpus. |

### Menu

**Import**

| Button | Action |
|---|---|
| Import files | Imports `.txt` files. |
| Import folder | Imports all `.txt` files from the selected folder. |
| Load corpus | Loads a corpus from a JSON file. |

**Indexing**

| Button | Action |
|---|---|
| Build index | Builds the index and fills the **Keywords** tab with words, TF-IDF scores and occurrence counts. |

**Analysis**

| Button | Action |
|---|---|
| Find similar | Finds documents similar to the selected one. |
| Clustering | Splits documents into groups of the most similar ones. The number of groups is set with the **Number of clusters** slider. |
| Frequency dictionary | Shows every word and how many times it occurs. |

**Other**

| Button | Action |
|---|---|
| Save | Saves the corpus to a JSON file that can be opened later. |
| Clear | Clears all fields and tabs. |

---

## Architecture

The application follows **MVVM** (Model–View–ViewModel):

- **Model**: documents, the corpus, the indexer, the analyzer (similarity) and the clustering algorithm, hidden behind interfaces.
- **View**: XAML windows and user controls with no business logic.
- **ViewModel**: state and commands that connect the View to the Model.

### Design patterns used

| Pattern | Where it is used |
|---|---|
| **MVVM** | Overall structure of the application. |
| **Command** | `RelayCommand` binds UI buttons to ViewModel actions and controls whether each button is enabled (`CanExecute`). |
| **Singleton** | `Corpus.Instance` is the single shared collection of documents. |
| **Dependency Injection** (constructor injection) | ViewModels receive services through interfaces such as `IIndexer`, `IAnalyzer`, `IKMeansCluster`, `IDialogService` and `IThemeService`. |
| **Service / Interface abstraction** | `IDialogService` hides message boxes and file dialogs from ViewModels, which keeps them testable and free of UI code. |
| **Observer** | `ObservableCollection` and `INotifyPropertyChanged` keep the UI in sync with the data. |

Other notable details:

- **Async operations.** Indexing, similarity search, clustering and file I/O run inside `Task.Run` with `async/await`, and UI updates are marshalled back to the UI thread.
- **Localization.** Strings come from `.resx` files and a custom markup extension (`LocExtension`) for use in XAML.
- **Value converters** adapt data for display in XAML bindings.

---

## Project structure

```
DocIndex/
├── Commands/       RelayCommand and other ICommand implementations
├── Converters/     XAML value converters
├── Localization/   Localization helpers (Loc, LocExtension)
├── Model/          Document, Corpus, indexer, analyzer, K-Means (+ interfaces)
├── Properties/     Resources (.resx) for localization, settings
├── Services/       Dialog and theme services
├── Styles/         Shared XAML styles
├── Themes/         Visual theme dictionaries
├── View/           Windows and user controls (XAML)
├── ViewModel/      ViewModels (Main, Management, Keywords, Stats, Frequency, ...)
├── App.xaml
└── DocIndex.sln
```

---

## Tech stack

- **Language:** C#
- **Framework:** .NET 8, WPF
- **Pattern:** MVVM
- **Serialization:** `System.Text.Json`
- **Algorithms:** TF-IDF, cosine similarity, K-Means

---

## Known limitations

- Only plain `.txt` files are supported.
- The saved corpus JSON stores **file paths**, not the document text. If the original files are moved or deleted, they will be reported as missing when the corpus is loaded.
- Folder import reads only the top-level folder (subfolders are not scanned).
- The number of clusters cannot be larger than the number of documents.

---

## Author

**Vladyslav Andreiev**: 3rd-year Software Engineering student, Lviv Polytechnic National University.

- GitHub: [@pelmenchik52](https://github.com/pelmenchik52)
- Email: vladzo16092006@gmail.com

<!-- Add a license if you want others to reuse the code, e.g. MIT, and mention it here. -->
