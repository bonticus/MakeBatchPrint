# MakeBatchPrint
Makes a Gold Batch Print Job from a list of IDs

Edit .config file to add db name, username, and pass

Program uses a new line to indicate a new number, so just paste directly from
excel/golden/toad/whatever.  No comma separation.  Header row can be included,
since a result for that id won't be found in gold.

Notes:
CWU uses padded ID numbers ('0' + cwu ID), remove 0 from code if you don't.
For the 'new cards only' option, it is looking at if:
  A card with the provided media type (050 for CWU) exists
  OR
  If a record exists in the card log

It will only print a card with a photo in the database.  CWU uses db image storage,
so it is checking the blob size to determine if a photo exists.

That's about it.  What more can be expected of a one-trick pony app?
