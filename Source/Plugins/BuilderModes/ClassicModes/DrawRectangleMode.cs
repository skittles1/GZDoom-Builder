﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    [EditMode(DisplayName = "Draw Rectangle Mode",
              SwitchAction = "drawrectanglemode",
              AllowCopyPaste = false,
              Volatile = true,
              Optional = false)]

    public class DrawRectangleMode : DrawGeometryMode
    {
        private HintLabel hintLabel;
        protected int bevelWidth;
        protected int currentBevelWidth;
        protected int subdivisions;

        protected int maxSubdivisions = 16;
        protected int minSubdivisions = 0;

        protected string undoName = "Rectangle draw";
        protected string shapeName = "rectangle";

        protected Vector2D start;
        protected Vector2D end;
        protected int width;
        protected int height;

        protected PixelColor cornersColor;

        public DrawRectangleMode() : base() {
            snaptogrid = true;
            cornersColor = General.Colors.BrightColors[new Random().Next(General.Colors.BrightColors.Length - 1)];
        }

        public override void Dispose() {
            if (!isdisposed && hintLabel != null)
                hintLabel.Dispose();

            base.Dispose();
        }
        
        override protected void Update() {
            PixelColor stitchcolor = General.Colors.Highlight;
            PixelColor losecolor = General.Colors.Selection;
            PixelColor color;

            snaptonearest = General.Interface.CtrlState ^ General.Interface.AutoMerge;

            DrawnVertex curp = GetCurrentPosition();
            float vsize = ((float)renderer.VertexSize + 1.0f) / renderer.Scale;
            float vsizeborder = ((float)renderer.VertexSize + 3.0f) / renderer.Scale;

            // Render drawing lines
            if (renderer.StartOverlay(true)) {
                color = snaptonearest ? stitchcolor : losecolor;
                
                if (points.Count == 1) {
                    updateReferencePoints(points[0], curp);
                    Vector2D[] shape = getShape(start, end);

                    //render shape
                    for (int i = 1; i < shape.Length; i++)
                        renderer.RenderLine(shape[i - 1], shape[i], LINE_THICKNESS, color, true);

                    //vertices
                    for (int i = 0; i < shape.Length; i++)
                        renderer.RenderRectangleFilled(new RectangleF(shape[i].x - vsize, shape[i].y - vsize, vsize * 2.0f, vsize * 2.0f), color, true);

                    //and labels
                    Vector2D[] labelCoords = new Vector2D[] { start, new Vector2D(end.x, start.y), end, new Vector2D(start.x, end.y), start };
                    for (int i = 1; i < 5; i++) {
                        labels[i - 1].Start = labelCoords[i - 1];
                        labels[i - 1].End = labelCoords[i];
                        renderer.RenderText(labels[i - 1].TextLabel);
                    }

                    //got beveled corners? 
                    if (shape.Length > 5) {
                        //render hint
                        if (width > 64 * vsize && height > 16 * vsize) {
                            float vPos = start.y + height / 2.0f;
                            hintLabel.Start = new Vector2D(start.x, vPos);
                            hintLabel.End = new Vector2D(end.x, vPos);
                            hintLabel.Text = getHintText();
                            renderer.RenderText(hintLabel.TextLabel);
                        }
                        
                        //and shape corners
                        for (int i = 0; i < 4; i++)
                            renderer.RenderRectangleFilled(new RectangleF(labelCoords[i].x - vsize, labelCoords[i].y - vsize, vsize * 2.0f, vsize * 2.0f), cornersColor, true);
                    }
                } else {
                    // Render vertex at cursor
                    renderer.RenderRectangleFilled(new RectangleF(curp.pos.x - vsize, curp.pos.y - vsize, vsize * 2.0f, vsize * 2.0f), color, true);
                }

                // Done
                renderer.Finish();
            }

            // Done
            renderer.Present();
        }

        protected virtual Vector2D[] getShape(Vector2D pStart, Vector2D pEnd) {
            //no shape
            if (pEnd.x == pStart.x && pEnd.y == pStart.y) {
                currentBevelWidth = 0;
                return new Vector2D[0];
            }

            //no corners
            if (bevelWidth == 0) {
                currentBevelWidth = 0;
                return new Vector2D[] { pStart, new Vector2D((int)pEnd.x, (int)pStart.y), pEnd, new Vector2D((int)pStart.x, (int)pEnd.y), pStart };
            }

            //got corners
            bool reverse = false;
            currentBevelWidth = Math.Min(Math.Abs(bevelWidth), Math.Min(width, height) / 2);
            
            if (bevelWidth < 0) {
                currentBevelWidth *= -1;
                reverse = true;
            }

            List<Vector2D> l_points = new List<Vector2D>();

            //top-left corner
            l_points.AddRange(getCornerPoints(pStart, currentBevelWidth, currentBevelWidth, !reverse));

            //top-right corner
            l_points.AddRange(getCornerPoints(new Vector2D(pEnd.x, pStart.y), -currentBevelWidth, currentBevelWidth, reverse));

            //bottom-right corner
            l_points.AddRange(getCornerPoints(pEnd, -currentBevelWidth, -currentBevelWidth, !reverse));

            //bottom-left corner
            l_points.AddRange(getCornerPoints(new Vector2D(pStart.x, pEnd.y), currentBevelWidth, -currentBevelWidth, reverse));

            //closing point
            l_points.Add(l_points[0]);

            Vector2D[] points = new Vector2D[l_points.Count];
            l_points.CopyTo(points);
            return points;
        }

        private Vector2D[] getCornerPoints(Vector2D startPoint, int bevel_width, int bevel_height, bool reverse) {
            Vector2D[] points;
            Vector2D center = (bevelWidth > 0 ? new Vector2D(startPoint.x + bevel_width, startPoint.y + bevel_height) : startPoint);
            float curAngle = (float)Math.PI;

            int steps = subdivisions + 2;
            points = new Vector2D[steps];
            float stepAngle = (float)Math.PI / 2.0f / (subdivisions + 1);

            for (int i = 0; i < steps; i++) {
                points[i] = new Vector2D(center.x + (float)Math.Sin(curAngle) * bevel_width, center.y + (float)Math.Cos(curAngle) * bevel_height);
                curAngle += stepAngle;
            }

            if (reverse) Array.Reverse(points);
            return points;
        }

        protected virtual string getHintText() {
            return "BVL: " + bevelWidth + "; SUB: " + subdivisions;
        }

        //update top-left and bottom-right points, which define drawing shape
        private void updateReferencePoints(DrawnVertex p1, DrawnVertex p2) {
            if (p1.pos.x < p2.pos.x) {
                start.x = p1.pos.x;
                end.x = p2.pos.x;
            } else {
                start.x = p2.pos.x;
                end.x = p1.pos.x;
            }

            if (p1.pos.y < p2.pos.y) {
                start.y = p1.pos.y;
                end.y = p2.pos.y;
            } else {
                start.y = p2.pos.y;
                end.y = p1.pos.y;
            }

            width = (int)(end.x - start.x);
            height = (int)(end.y - start.y);
        }

        // This draws a point at a specific location
        override public bool DrawPointAt(Vector2D pos, bool stitch, bool stitchline) {
            if (pos.x < General.Map.Config.LeftBoundary || pos.x > General.Map.Config.RightBoundary ||
                pos.y > General.Map.Config.TopBoundary || pos.y < General.Map.Config.BottomBoundary)
                return false;

            DrawnVertex newpoint = new DrawnVertex();
            newpoint.pos = pos;
            newpoint.stitch = true; //stitch
            newpoint.stitchline = stitchline;
            points.Add(newpoint);

            if (points.Count == 1) { //add point and labels
                labels.AddRange(new LineLengthLabel[] { new LineLengthLabel(), new LineLengthLabel(), new LineLengthLabel(), new LineLengthLabel() });
                hintLabel = new HintLabel();
                Update();
            } else if (points[0].pos == points[1].pos) { //nothing is drawn
                points = new List<DrawnVertex>();
                FinishDraw();
            } else {
                //create vertices for final shape. 
                updateReferencePoints(points[0], newpoint);
                points = new List<DrawnVertex>(); //clear points
                Vector2D[] shape = getShape(start, end);

                for (int i = 0; i < shape.Length; i++)
                    base.DrawPointAt(shape[i], true, true);

                FinishDraw();
            }
            return true;
        }

        override public void RemovePoint() {
            if (points.Count > 0) points.RemoveAt(points.Count - 1);
            if (labels.Count > 0) labels = new List<LineLengthLabel>();
            Update();
        }

//EVENTS
        override public void OnAccept() {
            Cursor.Current = Cursors.AppStarting;
            General.Settings.FindDefaultDrawSettings();

            // When we have a rectangle
            if (points.Count > 4) {
                // Make undo for the draw
                General.Map.UndoRedo.CreateUndo(undoName);

                // Make an analysis and show info
                string[] adjectives = new string[] { "gloomy", "sad", "unhappy", "lonely", "troubled", "depressed", "heartsick", "glum", "pessimistic", "bitter", "downcast" }; // aaand my english vocabulary ends here :)
                string word = adjectives[new Random().Next(adjectives.Length - 1)];
                string a = ((word[0] == 'a') || (word[0] == 'e') || (word[0] == 'o')) ? "an " : "a ";

                General.Interface.DisplayStatus(StatusType.Action, "Created " + a + word + " " + shapeName+".");

                // Make the drawing
                if (!Tools.DrawLines(points)) {
                    // Drawing failed
                    // NOTE: I have to call this twice, because the first time only cancels this volatile mode
                    General.Map.UndoRedo.WithdrawUndo();
                    General.Map.UndoRedo.WithdrawUndo();
                    return;
                }

                // Snap to map format accuracy
                General.Map.Map.SnapAllToAccuracy();

                // Clear selection
                General.Map.Map.ClearAllSelected();

                // Update cached values
                General.Map.Map.Update();

                // Edit new sectors?
                List<Sector> newsectors = General.Map.Map.GetMarkedSectors(true);
                if (BuilderPlug.Me.EditNewSector && (newsectors.Count > 0))
                    General.Interface.ShowEditSectors(newsectors);

                // Update the used textures
                General.Map.Data.UpdateUsedTextures();

                // Map is changed
                General.Map.IsChanged = true;
            }

            // Done
            Cursor.Current = Cursors.Default;

            // Return to original mode
            General.Editing.ChangeMode(General.Editing.PreviousStableMode.Name);
        }

//ACTIONS
        [BeginAction("increasesubdivlevel")]
        protected virtual void increaseSubdivLevel() {
            if (subdivisions < maxSubdivisions) {
                subdivisions++;
                Update();
            }
        }

        [BeginAction("decreasesubdivlevel")]
        protected virtual void decreaseSubdivLevel() {
            if (subdivisions > minSubdivisions) {
                subdivisions--;
                Update();
            }
        }

        [BeginAction("increasebevel")]
        protected virtual void increaseBevel() {
            if (currentBevelWidth == bevelWidth || bevelWidth < 0) {
                bevelWidth += General.Map.Grid.GridSize;
                Update();
            }
        }

        [BeginAction("decreasebevel")]
        protected virtual void decreaseBevel() {
            if (currentBevelWidth == bevelWidth || bevelWidth > 0) {
                bevelWidth -= General.Map.Grid.GridSize;
                Update();
            }
        }
    }
}