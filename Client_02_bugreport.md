# Client_02 Bug-Report

As version 2 of the Client software was being initially released, a critical datetime was relocated to a new folder during tuning. When the datetime was relocated, the format was accidentally changed from 24-hour format to 12-hour format. This resulted in some event records requalifying for read on the second execution after 1PM until midnight. 

This issue would not have affected the single-reading of an event file, but would have shown up in any kind of automating scenario.

Version 2 of the Firewall Monitor was pulled from public download when the bug was discovered, and until a fix could be tested. Rather than release a patched version of 02, a more finished Firewall Monitor 03 is being readied, including a tested version of the patch.
