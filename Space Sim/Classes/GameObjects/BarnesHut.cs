using GameObjects;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GameObjects
{
    public class BarnesHut
    {
        internal List<PointMass> MassPool = new List<PointMass>();
        internal Dictionary<PointMass, Leaf> LookUp = new Dictionary<PointMass, Leaf>();
        internal Quad TotalQuad;
        Branch Root;

        public BarnesHut(Vector2 Min, Vector2 Max)
        {
            TotalQuad = new Quad(Min, Max);
            Root = new Branch(null, TotalQuad);
        }

        internal void CalculateForces()
        {
            // recreate tree
            Clear();
            foreach (PointMass P in MassPool) Root.Insert(new Leaf(P), this);
            
            // from each leaf
            //foreach(PointMass P in  MassPool) LookUp[P].CalculateForce(P.Position, )
        }
        internal void Clear()
        {
            LookUp.Clear();
            Root = new Branch(null, TotalQuad);
        }
        internal void Add(PointMass P) => MassPool.Add(P);
        
        
        
        internal abstract class SubTree
        {
            internal Branch Parent;
            internal Quad Quad;
        }



        internal class Branch : SubTree
        {
            internal Quad[] Quads = new Quad[4];
            internal SubTree[] SubTrees = new SubTree[4];
            internal bool Split = false;
            public Branch(Branch Parent, Quad Quad)
            {
                this.Parent = Parent;
                this.Quad = Quad;

                Vector2 Mid = Quad.Min + (Quad.Max - Quad.Min) * 0.5f;
                // create new quads
                Quads[0] = new Quad(Quad.Min, Mid);
                Quads[1] = new Quad(Quad.Min.X, Mid.Y, Mid.X, Quad.Max.Y);
                Quads[2] = new Quad(Mid, Quad.Max);
                Quads[3] = new Quad(Mid.X, Quad.Min.Y, Quad.Max.X, Mid.Y);
            }
            internal void Insert(Leaf Leaf, BarnesHut Tree)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (Quads[i].Contains(Leaf.PointMass.Position))
                    {
                        switch (SubTrees[i])
                        {
                            case null:
                                // if null set to leaf
                                Leaf.Parent = this; // parent set because its a inserting directly to this object
                                SubTrees[i] = Leaf;
                                return;

                            case Branch _:
                                // if branch insert leaf into child branch
                                ((Branch)SubTrees[i]).Insert(Leaf, Tree);
                                return;

                            case Leaf _:
                                // If leaf convert to branch and insert new leaf into new child branch
                                Branch B = new Branch(SubTrees[i].Parent, SubTrees[i].Quad);
                                B.Insert((Leaf)SubTrees[i], Tree); // re-insert old leaf into new Branch
                                B.Insert(Leaf, Tree); // insert new leaf
                                SubTrees[i] = B;
                                return;
                        }
                    }
                }
            }
        }



        internal class Leaf : SubTree
        {
            internal PointMass PointMass;
            internal Leaf(PointMass P)
            {
                PointMass = P;
            }
            /*
            internal Vector2 CalculateForce(Vector2 Position, int Height)
            {
                if (Parent != null)
                {

                }
                else
                {

                }
            }
            */
        }
        
        
        
        internal struct Quad
        {
            internal Vector2 Min, Max;
            public Quad(Vector2 Min, Vector2 Max)
            {
                this.Min = Min;
                this.Max = Max;
            }
            public Quad(float MinX, float MinY, float MaxX, float MaxY) : this(new Vector2(MinX, MinY), new Vector2(MaxX, MaxY)) { }
            public bool Intersects(Quad that) => Min.X <= that.Max.X && Min.Y <= that.Max.Y && Max.X > that.Min.X && Max.Y > that.Min.Y;
            public bool Contains(Quad that) => that.Min.X >= Min.X && that.Min.Y >= Min.Y && that.Max.X <= Max.X && that.Max.Y <= Max.Y;
            public bool Contains(Vector2 Pos) => Pos.X >= Min.X && Pos.Y >= Min.Y && Pos.X < Max.X && Pos.Y < Max.Y;

        }
    }
}

