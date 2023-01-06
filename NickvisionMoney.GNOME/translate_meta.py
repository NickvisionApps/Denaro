#!/usr/bin/env python3

import os
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

script_dir = Path(__file__).parent
resx_dir = (script_dir / '../NickvisionMoney.Shared/Resources/').resolve()
install_prefix = sys.argv[1] if len(sys.argv) > 1 else '/usr'

regex = re.compile(r'Strings\.(.+)\.resx')
desktop_comments = []
meta_descriptions = []
for filename in os.listdir(resx_dir):
    regex_match = regex.search(filename)
    if regex_match:
        lang_code = regex_match.group(1)
        tree = ET.parse(f'{resx_dir}/{filename}')
        root = tree.getroot()
        for item in root.findall('./data'):
            if item.attrib['name'] == 'Description':
                text = item.find('value').text
                if text:
                    desktop_comments.append(f'Comment[{lang_code}]={text}')
                    meta_descriptions.append(f'    <p xml:lang="{lang_code}">\n      {text}\n    </p>')
desktop_comments.sort()
meta_descriptions.sort()

with open(f'{install_prefix}/share/applications/org.nickvision.money.desktop', 'r') as f:
    contents = f.readlines()
for i in range(len(contents)):
    if contents[i].startswith('Comment='):
        contents.insert(i + 1, "\n".join(desktop_comments) + "\n")
        break
with open(f'{install_prefix}/share/applications/org.nickvision.money.desktop', 'w') as f:
    contents = "".join(contents)
    f.write(contents)

with open(f'{install_prefix}/share/metainfo/org.nickvision.money.metainfo.xml', 'r') as f:
    contents = f.readlines()
for i in range(len(contents)):
    if contents[i].find('<description>') > -1:
        contents.insert(i + 4, "\n".join(meta_descriptions) + "\n")
        break
with open(f'{install_prefix}/share/metainfo/org.nickvision.money.metainfo.xml', 'w') as f:
    contents = "".join(contents)
    f.write(contents)
