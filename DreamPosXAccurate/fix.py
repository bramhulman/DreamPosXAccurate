import sys
for fname in ['Class/clsItem.vb', 'Class/clsPayment.vb']:
    with open(fname, 'r', encoding='utf-8') as f:
        content = f.read()
    
    content = content.replace('SafeSqlText(ex.Message)', 'ex.Message.Replace(\"\"\"\"\"\"\"\", \"\\\"\")')
    content = content.replace(\"ex.Message.Replace(''''', '\''''')\", 'ex.Message.Replace(\"\"\"\"\"\"\"\", \"\\\"\")')
    
    with open(fname, 'w', encoding='utf-8') as f:
        f.write(content)
