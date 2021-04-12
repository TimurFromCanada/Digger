﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Digger
{
    public class Terrain : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return true;
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Terrain.png";
        }
    }

    public class Player : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            var digger = new CreatureCommand { };

            switch (Game.KeyPressed) //Добавляем управление кнопками
            {
                case System.Windows.Forms.Keys.Left:
                    if (x - 1 >= 0)
                        digger.DeltaX = -1;
                    break;
                case System.Windows.Forms.Keys.Right:
                    if (x + 1 < Game.MapWidth)
                        digger.DeltaX = 1;
                    break;
                case System.Windows.Forms.Keys.Up:
                    if (y - 1 >= 0)
                        digger.DeltaY = -1;
                    break;
                case System.Windows.Forms.Keys.Down:
                    if (y + 1 < Game.MapHeight)
                        digger.DeltaY = 1;
                    break;
            }

            if (Game.Map[x + digger.DeltaX, y + digger.DeltaY] is Sack) // Ограничение на движение, если игрок пытается сдвинуть Sack
            {
                digger.DeltaX = 0;
                digger.DeltaY = 0;
            }
            return digger;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Monster) // Проверка на столкновение с монстром
            {
                return conflictedObject is Monster;
            }
                
            return conflictedObject is Sack;
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Digger.png";
        }
    }

    public class Sack : ICreature
    {
        public int CountSteps = 0;

        public CreatureCommand Act(int x, int y)
        {
            if (y + 1 < Game.MapHeight)
            {
                if (CountSteps > 0 && Game.Map[x, y + 1] is Player
                    || CountSteps > 0 && Game.Map[x, y + 1] is Monster
                    || Game.Map[x, y + 1] == null) //Проверяем что находиться внизу Sack и запускаем на падение, если ничего нет. 
                                                   //Убиваем Monster и Player, если они находяться снизу мешка
                {
                    CountSteps++; // Начинаем считать количество клеток, сколько падает Sack
                    return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
                }
            }

            if (CountSteps > 1 || y == Game.MapHeight)        // Если мешок падал больше одной клетки, превращаем его в Gold
            {
                return new CreatureCommand { TransformTo = new Gold() };
            }
            else
                CountSteps = 0;

            return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            return false;
        }

        public int GetDrawingPriority()
        {
            return 4;
        }

        public string GetImageFileName()
        {
            return "Sack.png";
        }
    }

    public class Gold : ICreature
    {
        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand();
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Player)
            {
                Game.Scores += 10;   // Добавляем 10 баллов, если игрок собирает золото
            }
                
            return true;
        }

        public int GetDrawingPriority()
        {
            return 3;
        }

        public string GetImageFileName()
        {
            return "Gold.png";
        }
    }

    public class Monster : ICreature
    {
        public bool PlayerOnTheMap() //Проверяем находиться ли Player на карте
        {
            for (var i = 0; i < Game.MapWidth; i++)
            {
                for (var j = 0; j < Game.MapHeight; j++)
                {
                    if (Game.Map[i, j] is Player)
                        return true;
                }
            }
                
            return false;
        }

        public int[] GetCoordinatesPlayer() //Находим координаты по которым находится Player
        {
            for (var i = 0; i < Game.MapWidth; i++)
            {
                for (var j = 0; j < Game.MapHeight; j++)
                {
                    if (Game.Map[i, j] is Player)
                        return new[] { i, j };
                }
            }
                
            return new int[0];
        }

        public bool GetNameObject(int x, int y) //Проверяем, что находиться в клетке куда идет Monster
        {
            return Game.Map[x, y] is Sack || Game.Map[x, y] is Monster || Game.Map[x, y] is Terrain;
        }

        public CreatureCommand Act(int x, int y)
        {
            var monster = new CreatureCommand { };

            if (PlayerOnTheMap())
            {
                if (GetCoordinatesPlayer()[0] < x) //Алгоритм нахождения кратчайшего пути до Player
                {
                    monster.DeltaX = -1;
                    if (!GetNameObject(x + monster.DeltaX, y + monster.DeltaY))
                        return monster;
                    {
                        monster.DeltaX = 0;
                        return monster;
                    }
                }

                if (GetCoordinatesPlayer()[0] > x)
                {
                    monster.DeltaX = 1;
                    if (!GetNameObject(x + monster.DeltaX, y + monster.DeltaY))
                        return monster;
                    {
                        monster.DeltaX = 0;
                        return monster;
                    }
                }

                if (GetCoordinatesPlayer()[1] < y)
                {
                    monster.DeltaY = -1;
                    if (!GetNameObject(x + monster.DeltaX, y + monster.DeltaY))
                        return monster;
                    {
                        monster.DeltaY = 0;
                        return monster;
                    }
                }

                if (GetCoordinatesPlayer()[1] > y)
                {
                    monster.DeltaY = 1;
                    if (!GetNameObject(x + monster.DeltaX, y + monster.DeltaY))
                        return monster;
                    {
                        monster.DeltaY = 0;
                        return monster;
                    }
                }
            }
            return monster;
        }

        public bool DeadInConflict(ICreature conflictedObject)
        {
            if (conflictedObject is Gold)
            {
                return conflictedObject is Monster;
            }
                
            return conflictedObject is Player ? false : true;
        }

        public int GetDrawingPriority()
        {
            return 2;
        }

        public string GetImageFileName()
        {
            return "Monster.png";
        }
    }
}