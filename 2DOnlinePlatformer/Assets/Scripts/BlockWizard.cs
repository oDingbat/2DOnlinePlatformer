using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlockWizard : MonoBehaviour {

	public bool go;

	public List<GameObject> blocks;
	public List<BlockInfo> blockInfos;

	[System.Serializable]
	public class BlockInfo {
		public bool start;
		public string blockName;
		public Sprite[] sprites;
	}
	
	private void OnDrawGizmosSelected () {
		if (go == true) {
			go = false;

			Sprite[] blockSpreadsheetSprites = Resources.LoadAll<Sprite>("Art/BlockSpreadsheet");
			blocks = GameObject.FindGameObjectsWithTag("Block").ToList();

			// Get Block Infos
			foreach (BlockInfo blockInfo in blockInfos) {
				if (blockInfo.start == true) {
					blockInfo.start = false;
					blockInfo.sprites = new Sprite[16];
					
					blockInfo.sprites[0] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_Solo"));
					blockInfo.sprites[1] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PillarBottom"));
					blockInfo.sprites[2] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PillarTop"));
					blockInfo.sprites[3] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PillarMiddle"));
					blockInfo.sprites[4] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PlatformLeft"));
					blockInfo.sprites[5] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_BottomLeft"));
					blockInfo.sprites[6] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_TopLeft"));
					blockInfo.sprites[7] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_MiddleLeft"));
					blockInfo.sprites[8] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PlatformRight"));
					blockInfo.sprites[9] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_BottomRight"));
					blockInfo.sprites[10] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_TopRight"));
					blockInfo.sprites[11] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_MiddleRight"));
					blockInfo.sprites[12] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_PlatformMiddle"));
					blockInfo.sprites[13] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_BottomMiddle"));
					blockInfo.sprites[14] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_TopMiddle"));
					blockInfo.sprites[15] = blockSpreadsheetSprites.Single(s => s.name == ("Block_" + blockInfo.blockName + "_MiddleMiddle"));
				}
			}

			// Set Block Sprites
			foreach (GameObject block in blocks) {
				int blockBinaryValue = 0;
				Block blockScript = block.GetComponent<Block>();

				if (blocks.Exists(b => (b.transform.position == (block.transform.position + new Vector3(0, 1, 0)))) == true) {        // Is there a block above this block?
					blockBinaryValue += 1;
				}

				if (blocks.Exists(b => (b.transform.position == (block.transform.position + new Vector3(0, -1, 0)))) == true) {        // Is there a block above this block?
					blockBinaryValue += 2;
				}

				if (blocks.Exists(b => (b.transform.position == (block.transform.position + new Vector3(1, 0, 0)))) == true) {        // Is there a block above this block?
					blockBinaryValue += 4;
				}

				if (blocks.Exists(b => (b.transform.position == (block.transform.position + new Vector3(-1, 0, 0)))) == true) {        // Is there a block above this block?
					blockBinaryValue += 8;
				}

				if (blockScript != null) {
					block.GetComponent<SpriteRenderer>().sprite = blockInfos.Find(b => b.blockName == blockScript.blockName).sprites[blockBinaryValue];
				} else {
					block.GetComponent<SpriteRenderer>().sprite = blockInfos[0].sprites[blockBinaryValue];
				}
			}
		}
	}


}
