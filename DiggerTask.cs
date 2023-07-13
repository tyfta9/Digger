using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;

namespace Digger
{
    //Напишите здесь классы Player, Terrain и другие.
    public static class ImageFileName
    {
        public static void AreEqual(string expected, string actual)
        {
            if (actual != expected)
                throw new Exception($"The image file name is wrong, is {actual} but should be {expected}");
        }    
    }

    public class Terrain : ICreature
    {
        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            ImageFileName.AreEqual("Terrain.png", actualName);
            return actualName;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }

        public int GetDrawingPriority()
        {
            return 10;
        }

        public bool DeadInConflict(ICreature creature)
        {
            return creature.GetType().Name == "Player";
        }
    }

    public class Player : ICreature
    {
        public string GetImageFileName()
        {
            return "Digger.png";
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            switch (Game.KeyPressed)
            {
                case Keys.Right:
                    if (x + 1 < Game.MapWidth && (Game.Map[x + 1, y] == null 
                        || Game.Map[x + 1, y].GetType().Name != "Sack"))
                        command.DeltaX++;
                    break;
                case Keys.Left:
                    if (x - 1 >= 0 && (Game.Map[x - 1, y] == null 
                        || Game.Map[x - 1, y].GetType().Name != "Sack"))
                        command.DeltaX--;
                    break;
                case Keys.Up:
                    if (y - 1 >= 0 && (Game.Map[x, y - 1] == null 
                        || Game.Map[x, y - 1].GetType().Name != "Sack"))
                        command.DeltaY--;
                    break;
                case Keys.Down:
                    if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] == null 
                        || Game.Map[x, y + 1].GetType().Name != "Sack"))
                        command.DeltaY++;
                    break;
            }
            return command;
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public bool DeadInConflict(ICreature creature)
        {
            var creatureName = creature.GetType().Name;
            return creatureName == "Sack" || creatureName == "Monster";
        }
    }

    public class Sack : ICreature
    {
        public int FlewCells = 0;
        public bool IsMoving()
        {
            return this.FlewCells > 0;
        }

        public bool DeadInConflict(ICreature creature)
        {
            return creature.GetType().Name == "Gold";
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            if (y + 1 < Game.MapHeight && y >= 0 && (Game.Map[x, y + 1] == null 
                || this.IsMoving() 
                && (Game.Map[x, y + 1].GetType().Name == "Player"
                || Game.Map[x, y + 1].GetType().Name == "Monster")))
            {
                command.DeltaY++;
                this.FlewCells++;
                if (this.FlewCells > 1 && (y + 1 == Game.MapHeight - 1
                    || y + 1 < Game.MapHeight && Game.Map[x, y + 2] != null 
                    && (Game.Map[x, y + 2].GetType().Name != "Player" 
                    && Game.Map[x, y + 2].GetType().Name != "Monster")))
                {
                    Game.Map[x, y + 1] = new Gold();
                }
            }
            else
                this.FlewCells = 0;
            return command;
        }
        
        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            ImageFileName.AreEqual("Sack.png", actualName);
            return actualName;
        }
    }

    public class Gold : ICreature
    {
        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            ImageFileName.AreEqual("Gold.png", actualName);
            return actualName;
        }

        public bool DeadInConflict(ICreature creature)
        {
            var creatureName = creature.GetType().Name;
            if ( creatureName == "Player")
            {
                Game.Scores += 10;
                return true;
            }
            return creatureName == "Monster";
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
        }
    }

    public class Cell
    {
        public int X;
        public int Y;
    }

    public class Monster : ICreature
    {
        public Cell Cell;
        public Cell GetTargetCell()
        {
            for (int x = 0; x < Game.MapWidth; x++)
                for (int y = 0; y < Game.MapHeight; y++)
                    if (Game.Map[x, y] != null && Game.Map[x, y].GetType().Name == "Player")
                        return new Cell() { X = x, Y = y };
            return new Cell() { X = -1, Y = -1 };
        }

        public bool SeesTarget(Cell cell)
        {
            return cell.X >= 0 && Game.Map[cell.X, cell.Y] != null
                && Game.Map[cell.X, cell.Y].GetType().Name == "Player";
        }

        public CreatureCommand GetCommandForMonster(Cell targetCell)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            if (this.Cell.Y + 1 < Game.MapHeight
                && targetCell.Y > this.Cell.Y
                && (Game.Map[this.Cell.X, this.Cell.Y + 1] == null
                || Game.Map[this.Cell.X, this.Cell.Y + 1].GetType().Name == "Gold"
                || Game.Map[this.Cell.X, this.Cell.Y + 1].GetType().Name == "Player"))
                command.DeltaY++;
            else if (this.Cell.Y > 0
                && targetCell.Y < this.Cell.Y
                && (Game.Map[this.Cell.X, this.Cell.Y - 1] == null
                || Game.Map[this.Cell.X, this.Cell.Y - 1].GetType().Name == "Gold"
                || Game.Map[this.Cell.X, this.Cell.Y - 1].GetType().Name == "Player"))
                command.DeltaY--;
            else if (this.Cell.X < Game.MapWidth
                && targetCell.X > this.Cell.X
                && (Game.Map[this.Cell.X + 1, this.Cell.Y] == null
                || Game.Map[this.Cell.X + 1, this.Cell.Y].GetType().Name == "Gold"
                || Game.Map[this.Cell.X + 1, this.Cell.Y].GetType().Name == "Player"))
                command.DeltaX++;
            else if (this.Cell.X > 0
                && targetCell.X < this.Cell.X
                && (Game.Map[this.Cell.X - 1, this.Cell.Y] == null
                || Game.Map[this.Cell.X - 1, this.Cell.Y].GetType().Name == "Gold"
                || Game.Map[this.Cell.X - 1, this.Cell.Y].GetType().Name == "Player"))
                command.DeltaX--;
            return command;
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            var targetCell = this.GetTargetCell();
            this.Cell = new Cell { X = x, Y = y };
            if (this.SeesTarget(targetCell))
                command = GetCommandForMonster(targetCell);
            return command;
        }

        public bool DeadInConflict(ICreature creature)
        {
            var creatureName = creature.GetType().Name;
            return creatureName == "Sack" || creatureName == "Monster";
        }

        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            ImageFileName.AreEqual("Monster.png", actualName);
            return actualName;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }
    }
}