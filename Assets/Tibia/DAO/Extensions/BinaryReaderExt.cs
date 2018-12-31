using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tibia.DAO.Extensions
{
    public static class BinaryReaderExt
    {
        enum BinaryTree {
            BINARYTREE_ESCAPE_CHAR = 0xFD,
            BINARYTREE_NODE_START = 0xFE,
            BINARYTREE_NODE_END = 0xFF
        };
        public static BinaryReader GetBinaryTree(this BinaryReader fin)
        {
            var b = (BinaryTree)fin.ReadByte();
            if (b != BinaryTree.BINARYTREE_NODE_START)
                throw new Exception(string.Format("failed to read node start (getBinaryTree): {0}", b));

            var bt = new BinaryReader(new MemoryStream(((MemoryStream)fin.BaseStream).GetBuffer()));
            bt.BaseStream.Position = fin.BaseStream.Position;
            return bt;
        }
        public static void SkipNodes(this BinaryReader fin)
        {

            while (true)
            {
                var b = (BinaryTree)fin.ReadByte();
                switch (b)
                {
                    case BinaryTree.BINARYTREE_NODE_START:
                        {
                            fin.SkipNodes();
                            break;
                        }
                    case BinaryTree.BINARYTREE_NODE_END:
                        return;
                    case BinaryTree.BINARYTREE_ESCAPE_CHAR:
                        fin.ReadByte();
                        break;
                    default:
                        break;
                }
            }
        }
        public static List<BinaryReader> GetChildren(this BinaryReader fin)
        {

            List<BinaryReader> children = new List<BinaryReader>();

            while (true)
            {
                var b = (BinaryTree)fin.ReadByte();
                switch (b)
                {
                    case BinaryTree.BINARYTREE_NODE_START:
                        {
                            BinaryReader node = new BinaryReader(fin.BaseStream);
                            children.Add(node);
                            node.SkipNodes();
                            break;
                        }
                    case BinaryTree.BINARYTREE_NODE_END:
                        return children;
                    case BinaryTree.BINARYTREE_ESCAPE_CHAR:
                        fin.ReadByte();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
