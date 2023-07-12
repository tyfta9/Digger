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
    public class Terrain : ICreature
    {
        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            var expectedName = "Terrain.png";
            if (actualName != expectedName)
                throw new Exception($"The image file name is wrong, is {actualName} but should be {expectedName}");
            return actualName;
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            return command;
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
            string creatureName = creature.GetType().Name;
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
            var expectedName = "Sack.png";
            if (actualName != expectedName)
                throw new Exception($"The image file name is wrong, is {actualName} but should be {expectedName}");
            return actualName;
        }
    }

    public class Gold : ICreature
    {
        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            var expectedName = "Gold.png";
            if (actualName != expectedName)
                throw new Exception($"The image file name is wrong, is {actualName} but should be {expectedName}");
            return actualName;
        }

        public bool DeadInConflict(ICreature creature)
        {
            string creatureName = creature.GetType().Name;
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
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            return command;
        }
    }

    public class Cell
    {
        public int X;
        public int Y;
    }

    public class Monster : ICreature
    {
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

        public CreatureCommand[] GetFastestRoute(CreatureCommand[] commands = null)
        {
            if (commands == null)
                commands = new CreatureCommand[Game.MapHeight * Game.MapWidth];
            return commands;
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand() { DeltaX = 0, DeltaY = 0 };
            var targetCell = this.GetTargetCell();
            //if (!Game.IsOver && this.SeesTarget(targetCell))
                
            return command;
        }

        public bool DeadInConflict(ICreature creature)
        {
            string creatureName = creature.GetType().Name;
            return creatureName == "Sack" || creatureName == "Monster";
        }

        public string GetImageFileName()
        {
            var actualName = $"{this.GetType().Name}.png";
            var expectedName = "Monster.png";
            if (actualName != expectedName)
                throw new Exception($"The image file name is wrong, is {actualName} but should be {expectedName}");
            return actualName;
        }

        public int GetDrawingPriority()
        {
            return 1;
        }
    }
}
