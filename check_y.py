import re
import os

scene_file = r"c:\Users\oyina\Desktop\unity labs\CampusNightShift\Assets\Scenes\GameLevel.unity"

if not os.path.exists(scene_file):
    print(f"Scene file not found at {scene_file}")
    exit()

with open(scene_file, 'r', encoding='utf-8') as f:
    data = f.read()

objects = data.split('--- !u!')

go_dict = {}
transform_dict = {}

for obj in objects:
    if obj.startswith('1 '): # GameObject
        id_match = re.match(r'1 &(\d+)', obj)
        name_match = re.search(r'm_Name:\s*(.*)', obj)
        if id_match and name_match:
            go_dict[id_match.group(1)] = name_match.group(1).strip()
    elif obj.startswith('4 '): # Transform
        id_match = re.match(r'4 &(\d+)', obj)
        go_match = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', obj)
        pos_match = re.search(r'm_LocalPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^,]+),\s*z:\s*([^}]+)\}', obj)
        if id_match and go_match and pos_match:
            transform_dict[go_match.group(1)] = pos_match.group(2)
            
    elif obj.startswith('1001 '): # Prefab instance
        name_match = re.search(r'propertyPath: m_Name\n\s*value:\s*(.*)', obj)
        y_match = re.search(r'propertyPath: m_LocalPosition\.y\n\s*value:\s*([^\n]+)', obj)
        if name_match and y_match:
            go_dict["prefab_" + name_match.group(1)] = name_match.group(1).strip()
            transform_dict["prefab_" + name_match.group(1)] = y_match.group(1).strip()

print("--- STRUCTURAL POSITIONS (Y-AXIS) ---")
levels = set()
for go_id, name in go_dict.items():
    if go_id in transform_dict:
        y = float(transform_dict[go_id])
        if any(keyword in name.lower() for keyword in ["floor", "wall", "room", "player", "plane", "cube", "desk", "locker"]):
            print(f"{name:<25}: Y = {y:.3f}")
            if "floor" in name.lower() or "plane" in name.lower():
                levels.add(y)

print(f"\nDetected Floor Levels: {levels}")
if len(levels) > 1:
    print("WARNING: You seem to have floors at multiple different heights!")
else:
    print("SUCCESS: Floors seem to be consistently leveled.")
