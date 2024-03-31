## [Project-Name]

Description of what it does

[docs :open_book:](https://dazinator.github.io/[Project-Name]/)

### Serving the Docs Locally

Make sure you have python 3 installed, then run the following commands in the repo root directory (you may have to run as administrator)

```sh
  pip install --upgrade pip setuptools wheel
  pip install -r docs/requirements.txt 
```

You can now build the docs site, and start the mkdocs server for live preview:

```
mkdocs build
mkdocs serve
```

Browse to the docs site on `http://127.0.0.1:8000/` - the site will reload as you make changes.

For more information including features, see [mkdocs-material](https://squidfunk.github.io/mkdocs-material/)