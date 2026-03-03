const fs = require('fs');

const sceneFile = 'Assets/Scenes/GameLevel.unity';

if (!fs.existsSync(sceneFile)) {
    console.error(`Scene file not found at ${sceneFile}`);
    process.exit(1);
}

let data = fs.readFileSync(sceneFile, 'utf-8');
const objects = data.split('--- !u!');

const goDict = {};
const transformMapping = {}; // GameObject ID -> Transform ID

// 1. Find the GameObject IDs and corresponding Transform IDs for the Waypoints
for (let i = 0; i < objects.length; i++) {
    const obj = objects[i];
    if (obj.startsWith('1 ')) { // GameObject
        const idMatch = obj.match(/^1 &(\d+)/);
        const nameMatch = obj.match(/m_Name:\s*(.*)/);

        if (idMatch && nameMatch) {
            const name = nameMatch[1].trim();
            if (name.includes('Waypoint_')) {
                goDict[idMatch[1]] = name;
            }
        }
    } else if (obj.startsWith('4 ')) { // Transform
        const idMatch = obj.match(/^4 &(\d+)/);
        const goMatch = obj.match(/m_GameObject:\s*\{fileID:\s*(\d+)\}/);

        if (idMatch && goMatch) {
            transformMapping[goMatch[1]] = idMatch[1]; // Map GO ID -> Transform ID
        }
    }
}

console.log("Found Waypoints:", goDict);

// 2. The exact mathematical coordinates for the 4 corners of the "Loop"
// Assuming center is roughly (0,0) and hallways extend ~12m.
// We will set them at (0, 0), (0, 12), (-12, 12), (-12, 0) based on typical layout.
const targetPositions = {
    'Waypoint_1_South': { x: 0, y: 0, z: 0 },
    'Waypoint_2_West': { x: -12, y: 0, z: 0 },
    'Waypoint_3_North': { x: -12, y: 0, z: 12 },
    'Waypoint_4_East': { x: 0, y: 0, z: 12 }
};

let modifiedCount = 0;

// 3. Regex replace the LocalPosition for each found Transform block
for (const [goId, name] of Object.entries(goDict)) {
    const transId = transformMapping[goId];
    if (!transId) continue;

    // Find a matching hardcoded target, or just default to 0,0,0 if name is slightly different
    let target = targetPositions[name];
    if (!target) {
        // Fallback matching
        if (name.includes('1')) target = targetPositions['Waypoint_1_South'];
        else if (name.includes('2')) target = targetPositions['Waypoint_2_West'];
        else if (name.includes('3')) target = targetPositions['Waypoint_3_North'];
        else if (name.includes('4')) target = targetPositions['Waypoint_4_East'];
        else target = { x: 0, y: 0, z: 0 };
    }

    console.log(`Setting ${name} (Transform ID: ${transId}) to X:${target.x}, Y:${target.y}, Z:${target.z}`);

    // Create a precise regex to find the transform block by its ID and then replace its position
    const blockRegex = new RegExp(`(^4 &${transId}\\b[\\s\\S]*?m_LocalPosition: )\\{x: [^,]+, y: [^,]+, z: [^\\}]+\\}`, "m");
    data = data.replace(blockRegex, `$1{x: ${target.x.toFixed(5)}, y: ${target.y.toFixed(5)}, z: ${target.z.toFixed(5)}}`);
    modifiedCount++;
}

fs.writeFileSync(sceneFile, data);
console.log(`Successfully hardcoded ${modifiedCount} waypoints into the Unity Scene.`);
