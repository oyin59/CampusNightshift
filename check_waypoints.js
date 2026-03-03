const fs = require('fs');

const sceneFile = 'Assets/Scenes/GameLevel.unity';

if (!fs.existsSync(sceneFile)) {
    console.error(`Scene file not found at ${sceneFile}`);
    process.exit(1);
}

const data = fs.readFileSync(sceneFile, 'utf-8');
const objects = data.split('--- !u!');

const goDict = {};
const transformDict = {};

// Parse GameObjects and Transforms
for (const obj of objects) {
    if (obj.startsWith('1 ')) { // GameObject
        const idMatch = obj.match(/^1 &(\d+)/);
        const nameMatch = obj.match(/m_Name:\s*(.*)/);
        if (idMatch && nameMatch) {
            goDict[idMatch[1]] = nameMatch[1].trim();
        }
    } else if (obj.startsWith('4 ')) { // Transform
        const idMatch = obj.match(/^4 &(\d+)/);
        const goMatch = obj.match(/m_GameObject:\s*\{fileID:\s*(\d+)\}/);
        const posMatch = obj.match(/m_LocalPosition:\s*\{x:\s*([^,]+),\s*y:\s*([^,]+),\s*z:\s*([^}]+)\}/);
        if (idMatch && goMatch && posMatch) {
            transformDict[goMatch[1]] = {
                x: parseFloat(posMatch[1]),
                y: parseFloat(posMatch[2]),
                z: parseFloat(posMatch[3])
            };
        }
    } else if (obj.startsWith('1001 ')) { // PrefabInstance
        const nameMatch = obj.match(/propertyPath: m_Name\s*value:\s*(.*)/);
        const yMatch = obj.match(/propertyPath: m_LocalPosition\.y\s*value:\s*([^\r\n]+)/);
        if (nameMatch && yMatch) {
            const tempId = "prefab_" + nameMatch[1].trim();
            goDict[tempId] = nameMatch[1].trim();
            transformDict[tempId] = {
                x: 0, // Simplified for prefabs
                y: parseFloat(yMatch[1]),
                z: 0  // Simplified for prefabs
            };
        }
    }
}

console.log("--- WAYPOINT & AI POSITIONS ---");

let waypointCount = 0;
for (const [goId, name] of Object.entries(goDict)) {
    if (name.toLowerCase().includes('waypoint') || name.toLowerCase().includes('guard') || name.toLowerCase().includes('ai')) {
        const trans = transformDict[goId];
        if (trans) {
            console.log(`${name.padEnd(25)}: X=${trans.x.toFixed(2)}, Y=${trans.y.toFixed(2)}, Z=${trans.z.toFixed(2)}`);
            if (name.toLowerCase().includes('waypoint')) waypointCount++;
        }
    }
}

console.log(`\nFound ${waypointCount} waypoints.`);
