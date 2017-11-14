﻿using System;
using System.Collections.Generic;

namespace DragonBones
{
    /**
     * 插槽，附着在骨骼上，控制显示对象的显示状态和属性。
     * 一个骨骼上可以包含多个插槽。
     * 一个插槽中可以包含多个显示对象，同一时间只能显示其中的一个显示对象，但可以在动画播放的过程中切换显示对象实现帧动画。
     * 显示对象可以是普通的图片纹理，也可以是子骨架的显示容器，网格显示对象，还可以是自定义的其他显示对象。
     * @see dragonBones.Armature
     * @see dragonBones.Bone
     * @see dragonBones.SlotData
     * @version DragonBones 3.0
     * @language zh_CN
     */
    public abstract class Slot : TransformObject
    {
        /**
         * @language zh_CN
         * 显示对象受到控制的动画状态或混合组名称，设置为 null 则表示受所有的动画状态控制。
         * @default null
         * @see DragonBones.AnimationState#displayControl
         * @see DragonBones.AnimationState#name
         * @see DragonBones.AnimationState#group
         * @version DragonBones 4.5
         */
        public string displayController;
        /**
         * @private
         */
        protected bool _displayDirty;
        /**
         * @private
         */
        protected bool _zOrderDirty;
        /**
         * @private
         */
        protected bool _visibleDirty;
        /**
         * @private
         */
        protected bool _blendModeDirty;
        /**
         * @internal
         * @private
         */
        internal bool _colorDirty;
        /**
         * @internal
         * @private
         */
        internal bool _meshDirty;
        /**
         * @private
         */
        protected bool _transformDirty;
        /**
         * @private
         */
        protected bool _visible;
        /**
         * @private
         */
        protected BlendMode _blendMode;
        /**
         * @private
         */
        protected int _displayIndex;
        /**
         * @private
         */
        protected int _animationDisplayIndex;
        /**
         * @internal
         * @private
         */
        internal int _zOrder;
        /**
         * @private
         */
        protected int _cachedFrameIndex;
        /**
         * @internal
         * @private
         */
        internal float _pivotX;
        /**
         * @internal
         * @private
         */
        internal float _pivotY;
        /**
         * @private
         */
        protected readonly Matrix _localMatrix = new Matrix();
        /**
         * @private
         */
        internal readonly ColorTransform _colorTransform = new ColorTransform();
        /**
         * @private
         */
        internal readonly List<float> _ffdVertices = new List<float>();
        /**
         * @private
         */
        internal readonly List<DisplayData> _displayDatas = new List<DisplayData>();
        /**
         * @private
         */
        protected readonly List<object> _displayList = new List<object>();
        /**
         * @private
         */
        protected readonly List<Bone> _meshBones = new List<Bone>();
        /**
         * @readonly
         */
        internal SlotData _slotData;
        /**
         * @private
         */
        protected List<DisplayData> _rawDisplayDatas;
        /**
         * @private
         */
        protected DisplayData _displayData;
        /**
         * @private
         */
        protected TextureData _textureData;
        /**
         * @internal
         * @private
         */
        internal MeshDisplayData _meshData;
        /**
         * @private
         */
        protected BoundingBoxData _boundingBoxData;
        /**
         * @private
         */
        protected object _rawDisplay;
        /**
         * @private
         */
        protected object _meshDisplay;
        /**
         * @private
         */
        protected object _display;
        /**
         * @private
         */
        protected Armature _childArmature;
        /**
         * @internal
         * @private
         */
        internal List<int> _cachedFrameIndices = new List<int>();
        /**
         * @private
         */
        public Slot()
        {
        }
        /**
         * @private
         */
        protected override void _OnClear()
        {
            base._OnClear();

            var disposeDisplayList = new List<object>();
            for (int i = 0, l = _displayList.Count; i < l; ++i)
            {
                var eachDisplay = _displayList[i];
                if ( eachDisplay != _rawDisplay && eachDisplay != _meshDisplay && !disposeDisplayList.Contains(eachDisplay))
                {
                    disposeDisplayList.Add(eachDisplay);
                }
            }

            for (int i = 0, l = disposeDisplayList.Count; i < l; ++i)
            {
                var eachDisplay = disposeDisplayList[i];
                if (eachDisplay is Armature)
                {
                    (eachDisplay as Armature).Dispose();
                }
                else
                {
                    _DisposeDisplay(eachDisplay);
                }
            }

            if (this._meshDisplay != null && this._meshDisplay != this._rawDisplay)
            { 
                // May be _meshDisplay and _rawDisplay is the same one.
                this._DisposeDisplay(this._meshDisplay);
            }

            if (this._rawDisplay != null)
            {
                this._DisposeDisplay(this._rawDisplay);
            }

            this.displayController = null;

            this._displayDirty = false;
            this._zOrderDirty = false;
            this._blendModeDirty = false;
            this._colorDirty = false;
            this._meshDirty = false;
            this._transformDirty = false;
            this._visible = true;
            this._blendMode = BlendMode.Normal;
            this._displayIndex = -1;
            this._animationDisplayIndex = -1;
            this._zOrder = 0;
            this._cachedFrameIndex = -1;
            this._pivotX = 0.0f;
            this._pivotY = 0.0f;
            this._localMatrix.Identity();
            this._colorTransform.Identity();
            this._ffdVertices.Clear();
            this._displayList.Clear();
            this._displayDatas.Clear();
            this._meshBones.Clear();
            this._slotData = null; //
            this._rawDisplayDatas = null; //
            this._displayData = null;
            this._textureData = null;
            this._meshData = null;
            this._boundingBoxData = null;
            this._rawDisplay = null;
            this._meshDisplay = null;
            this._display = null;
            this._childArmature = null;
            this._cachedFrameIndices = null;
        }

        /**
         * @private
         */
        protected abstract void _InitDisplay(object value);
        /**
         * @private
         */
        protected abstract void _DisposeDisplay(object value);
        /**
         * @private
         */
        protected abstract void _OnUpdateDisplay();
        /**
         * @private
         */
        protected abstract void _AddDisplay();
        /**
         * @private
         */
        protected abstract void _ReplaceDisplay(object value);
        /**
         * @private
         */
        protected abstract void _RemoveDisplay();
        /**
         * @private
         */
        protected abstract void _UpdateZOrder();
        /**
         * @private
         */
        internal abstract void _UpdateVisible();
        /**
         * @private
         */
        protected abstract void _UpdateBlendMode();
        /**
         * @private
         */
        protected abstract void _UpdateColor();
        /**
         * @private
         */
        protected abstract void _UpdateFrame();
        /**
         * @private
         */
        protected abstract void _UpdateMesh();
        /**
         * @private
         */
        protected abstract void _UpdateTransform(bool isSkinnedMesh);

        /**
         * @private
         */
        protected void _UpdateDisplayData()
        {
            var prevDisplayData = this._displayData;
            var prevTextureData = this._textureData;
            var prevMeshData = this._meshData;
            var rawDisplayData = this._displayIndex >= 0 && this._rawDisplayDatas != null && this._displayIndex < this._rawDisplayDatas.Count ? this._rawDisplayDatas[this._displayIndex] : null;

            if (this._displayIndex >= 0 && this._displayIndex < this._displayDatas.Count)
            {
                this._displayData = this._displayDatas[this._displayIndex];
            }
            else
            {
                this._displayData = null;
            }

            // Update texture and mesh data.
            if (this._displayData != null)
            {
                if (this._displayData.type == DisplayType.Image || this._displayData.type == DisplayType.Mesh)
                {
                    this._textureData = (this._displayData as ImageDisplayData).texture;
                    if (this._displayData.type == DisplayType.Mesh)
                    {
                        this._meshData = this._displayData as MeshDisplayData;
                    }
                    else if (rawDisplayData != null && rawDisplayData.type == DisplayType.Mesh)
                    {
                        this._meshData = rawDisplayData as MeshDisplayData;
                    }
                    else
                    {
                        this._meshData = null;
                    }
                }
                else
                {
                    this._textureData = null;
                    this._meshData = null;
                }
            }
            else
            {
                this._textureData = null;
                this._meshData = null;
            }

            // Update bounding box data.
            if (this._displayData != null && this._displayData.type == DisplayType.BoundingBox)
            {
                this._boundingBoxData = (this._displayData as BoundingBoxDisplayData).boundingBox;
            }
            else if (rawDisplayData != null && rawDisplayData.type == DisplayType.BoundingBox)
            {
                this._boundingBoxData = (rawDisplayData as BoundingBoxDisplayData).boundingBox;
            }
            else
            {
                this._boundingBoxData = null;
            }

            if (this._displayData != prevDisplayData || this._textureData != prevTextureData || this._meshData != prevMeshData)
            {
                // Update pivot offset.
                if (this._meshData != null)
                {
                    this._pivotX = 0.0f;
                    this._pivotY = 0.0f;
                }
                else if (this._textureData != null)
                {
                    var imageDisplayData = this._displayData as ImageDisplayData;
                    var scale = this._textureData.parent.scale * this._armature._armatureData.scale;
                    var frame = this._textureData.frame;

                    this._pivotX = imageDisplayData.pivot.x;
                    this._pivotY = imageDisplayData.pivot.y;

                    var rect = frame != null ? frame : this._textureData.region;
                    var width = rect.width;
                    var height = rect.height;

                    if (this._textureData.rotated && frame == null)
                    {
                        width = rect.height;
                        height = rect.width;
                    }

                    this._pivotX *= width * scale;
                    this._pivotY *= height * scale;

                    if (frame != null)
                    {
                        this._pivotX += frame.x * scale;
                        this._pivotY += frame.y * scale;
                    }
                }
                else
                {
                    this._pivotX = 0.0f;
                    this._pivotY = 0.0f;
                }

                // Update mesh bones and ffd vertices.
                if (this._meshData != prevMeshData)
                {
                    if (this._meshData != null)// && this._meshData === this._displayData
                    {
                        if (this._meshData.weight != null)
                        {
                            this._ffdVertices.ResizeList(this._meshData.weight.count * 2);
                            this._meshBones.ResizeList(this._meshData.weight.bones.Count);

                            for (int i = 0, l = this._meshBones.Count; i < l; ++i)
                            {
                                this._meshBones[i] = this._armature.GetBone(this._meshData.weight.bones[i].name);
                            }
                        }
                        else
                        {
                            var vertexCount = this._meshData.parent.parent.parent.intArray[this._meshData.offset + (int)BinaryOffset.MeshVertexCount];
                            this._ffdVertices.ResizeList(vertexCount * 2);
                            this._meshBones.Clear();
                        }

                        for (int i = 0, l = this._ffdVertices.Count; i < l; ++i)
                        {
                            this._ffdVertices[i] = 0.0f;
                        }

                        this._meshDirty = true;
                    }
                    else
                    {
                        this._ffdVertices.Clear();
                        this._meshBones.Clear();
                    }
                }
                else if (this._meshData != null && this._textureData != prevTextureData)
                {
                    // Update mesh after update frame.
                    this._meshDirty = true;
                }

                if (this._displayData != null && rawDisplayData != null && this._displayData != rawDisplayData && this._meshData == null)
                {
                    rawDisplayData.transform.ToMatrix(Slot._helpMatrix);
                    Slot._helpMatrix.Invert();
                    Slot._helpMatrix.TransformPoint(0.0f, 0.0f, Slot._helpPoint);
                    this._pivotX -= Slot._helpPoint.x;
                    this._pivotY -= Slot._helpPoint.y;

                    this._displayData.transform.ToMatrix(Slot._helpMatrix);
                    Slot._helpMatrix.Invert();
                    Slot._helpMatrix.TransformPoint(0.0f, 0.0f, Slot._helpPoint);
                    this._pivotX += Slot._helpPoint.x;
                    this._pivotY += Slot._helpPoint.y;
                }

                // Update original transform.
                if (rawDisplayData != null)
                {
                    this.origin = rawDisplayData.transform;
                }
                else if (this._displayData != null)
                {
                    this.origin = this._displayData.transform;
                }

                this._displayDirty = true;
                this._transformDirty = true;
            }
        }

        /**
         * @private
         */
        protected void _UpdateDisplay()
        {
            var prevDisplay = this._display != null ? this._display : this._rawDisplay;
            var prevChildArmature = this._childArmature;

            // Update display and child armature.
            if (this._displayIndex >= 0 && this._displayIndex < this._displayList.Count)
            {
                this._display = this._displayList[this._displayIndex];
                if (this._display != null && this._display is Armature)
                {
                    this._childArmature = this._display as Armature;
                    this._display = this._childArmature.display;
                }
                else
                {
                    this._childArmature = null;
                }
            }
            else
            {
                this._display = null;
                this._childArmature = null;
            }

            // Update display.
            var currentDisplay = this._display != null ? this._display : this._rawDisplay;
            if (currentDisplay != prevDisplay)
            {
                this._OnUpdateDisplay();
                this._ReplaceDisplay(prevDisplay);

                this._visibleDirty = true;
                this._blendModeDirty = true;
                this._colorDirty = true;
            }

            // Update frame.
            if (currentDisplay == this._rawDisplay || currentDisplay == this._meshDisplay)
            {
                this._UpdateFrame();
            }

            // Update child armature.
            if (this._childArmature != prevChildArmature)
            {
                if (prevChildArmature != null)
                {
                    // Update child armature parent.
                    prevChildArmature._parent = null; 
                    prevChildArmature.clock = null;
                    if (prevChildArmature.inheritAnimation)
                    {
                        prevChildArmature.animation.Reset();
                    }
                }

                if (this._childArmature != null)
                {
                    // Update child armature parent.
                    this._childArmature._parent = this; 
                    this._childArmature.clock = this._armature.clock;
                    if (this._childArmature.inheritAnimation)
                    {
                        // Set child armature cache frameRate.
                        if (this._childArmature.cacheFrameRate == 0)
                        {
                            var cacheFrameRate = this._armature.cacheFrameRate;
                            if (cacheFrameRate != 0)
                            {
                                this._childArmature.cacheFrameRate = cacheFrameRate;
                            }
                        }

                        // Child armature action.
                        List<ActionData> actions = null;
                        if (this._displayData != null && this._displayData.type == DisplayType.Armature)
                        {
                            actions = (this._displayData as ArmatureDisplayData).actions;
                        }
                        else
                        {
                            var rawDisplayData = this._displayIndex >= 0 && this._rawDisplayDatas != null && this._displayIndex < this._rawDisplayDatas.Count ? this._rawDisplayDatas[this._displayIndex] : null;
                            if (rawDisplayData != null && rawDisplayData.type == DisplayType.Armature)
                            {
                                actions = (rawDisplayData as ArmatureDisplayData).actions;
                            }
                        }

                        if (actions != null && actions.Count > 0)
                        {
                            foreach (var action in actions)
                            {
                                this._childArmature._BufferAction(action, false); // Make sure default action at the beginning.
                            }
                        }
                        else
                        {
                            this._childArmature.animation.Play();
                        }
                    }
                }
            }
        }

        /**
         * @private
         */
        protected void _UpdateGlobalTransformMatrix(bool isCache)
        {
            this.globalTransformMatrix.CopyFrom(this._localMatrix);
            this.globalTransformMatrix.Concat(this._parent.globalTransformMatrix);
            if (isCache)
            {
                this.global.FromMatrix(this.globalTransformMatrix);
            }
            else
            {
                this._globalDirty = true;
            }
        }

        /**
         * @private
         */
        protected bool _IsMeshBonesUpdate()
        {
            foreach (var bone in this._meshBones)
            {
                if (bone != null && bone._childrenTransformDirty)
                {
                    return true;
                }
            }

            return false;
        }
        /**
         * @internal
         * @private
         */
        internal override void  _SetArmature(Armature value = null)
        {
            if (this._armature == value)
            {
                return;
            }

            if (this._armature != null)
            {
                this._armature._RemoveSlotFromSlotList(this);
            }

            this._armature = value; //

            this._OnUpdateDisplay();

            if (this._armature != null)
            {
                this._armature._AddSlotToSlotList(this);
                this._AddDisplay();
            }
            else
            {
                this._RemoveDisplay();
            }
        }

        /**
         * @internal
         * @private
         */
        internal bool _SetDisplayIndex(int value, bool isAnimation = false)
        {
            if (isAnimation)
            {
                if (this._animationDisplayIndex == value)
                {
                    return false;
                }

                this._animationDisplayIndex = value;
            }

            if (this._displayIndex == value)
            {
                return false;
            }

            this._displayIndex = value;
            this._displayDirty = true;

            this._UpdateDisplayData();

            return this._displayDirty;
        }

        /**
         * @internal
         * @private
         */
        internal bool _SetZorder(int value)
        {
            if (this._zOrder == value)
            {
                //return false;
            }

            this._zOrder = value;
            this._zOrderDirty = true;

            return this._zOrderDirty;
        }

        /**
         * @internal
         * @private
         */
        internal bool _SetColor(ColorTransform value)
        {
            this._colorTransform.CopyFrom(value);
            this._colorDirty = true;

            return this._colorDirty;
        }
        /**
         * @internal
         * @private
         */
        internal bool _SetDisplayList(List<object> value)
        {
            if (value != null && value.Count > 0)
            {
                if (this._displayList.Count != value.Count)
                {
                    this._displayList.ResizeList(value.Count);
                }

                for (int i = 0, l = value.Count; i < l; ++i)
                { 
                    // Retain input render displays.
                    var eachDisplay = value[i];
                    if (eachDisplay != null &&
                        eachDisplay != this._rawDisplay &&
                        eachDisplay != this._meshDisplay &&
                        !(eachDisplay is Armature) && this._displayList.IndexOf(eachDisplay) < 0)
                    {
                        this._InitDisplay(eachDisplay);
                    }

                    this._displayList[i] = eachDisplay;
                }
            }
            else if (this._displayList.Count > 0)
            {
                this._displayList.Clear();
            }

            if (this._displayIndex >= 0 && this._displayIndex < this._displayList.Count)
            {
                this._displayDirty = this._display != this._displayList[this._displayIndex];
            }
            else
            {
                this._displayDirty = this._display != null;
            }

            this._UpdateDisplayData();

            return this._displayDirty;
        }

        /**
         * @private
         */
        internal void Init(SlotData slotData, List<DisplayData> displayDatas, object rawDisplay, object meshDisplay)
        {
            if (this._slotData != null)
            {
                return;
            }

            this._slotData = slotData;
            //
            this._visibleDirty = true;
            this._blendModeDirty = true;
            this._colorDirty = true;
            this._blendMode = this._slotData.blendMode;
            this._zOrder = this._slotData.zOrder;
            this._colorTransform.CopyFrom(this._slotData.color);
            this._rawDisplay = rawDisplay;
            this._meshDisplay = meshDisplay;

            this.rawDisplayDatas = displayDatas;
        }

        /**
         * @internal
         * @private
         */
        internal void Update(int cacheFrameIndex)
        {
            if (this._displayDirty)
            {
                this._displayDirty = false;
                this._UpdateDisplay();

                if (this._transformDirty)
                {
                    // Update local matrix. (Only updated when both display and transform are dirty.)
                    if (this.origin != null)
                    {
                        this.global.CopyFrom(this.origin).Add(this.offset).ToMatrix(this._localMatrix);
                    }
                    else
                    {
                        this.global.CopyFrom(this.offset).ToMatrix(this._localMatrix);
                    }
                }
            }

            if (this._zOrderDirty)
            {
                this._zOrderDirty = false;
                this._UpdateZOrder();
            }

            if (cacheFrameIndex >= 0 && this._cachedFrameIndices != null)
            {
                var cachedFrameIndex = this._cachedFrameIndices[cacheFrameIndex];

                if (cachedFrameIndex >= 0 && this._cachedFrameIndex == cachedFrameIndex)
                { 
                    // Same cache.
                    this._transformDirty = false;
                }
                else if (cachedFrameIndex >= 0)
                { 
                    // Has been Cached.
                    this._transformDirty = true;
                    this._cachedFrameIndex = cachedFrameIndex;
                }
                else if (this._transformDirty || this._parent._childrenTransformDirty)
                { 
                    // Dirty.
                    this._transformDirty = true;
                    this._cachedFrameIndex = -1;
                }
                else if (this._cachedFrameIndex >= 0)
                { 
                    // Same cache, but not set index yet.
                    this._transformDirty = false;
                    this._cachedFrameIndices[cacheFrameIndex] = this._cachedFrameIndex;
                }
                else
                { 
                    // Dirty.
                    this._transformDirty = true;
                    this._cachedFrameIndex = -1;
                }
            }
            else if (this._transformDirty || this._parent._childrenTransformDirty)
            { 
                // Dirty.
                cacheFrameIndex = -1;
                this._transformDirty = true;
                this._cachedFrameIndex = -1;
            }

            if (this._display == null)
            {
                return;
            }

            if (this._visibleDirty)
            {
                this._visibleDirty = false;
                this._UpdateVisible();
            }

            if (this._blendModeDirty)
            {
                this._blendModeDirty = false;
                this._UpdateBlendMode();
            }

            if (this._colorDirty)
            {
                this._colorDirty = false;
                this._UpdateColor();
            }

            if (this._meshData != null && this._display == this._meshDisplay)
            {
                var isSkinned = this._meshData.weight != null;

                if (this._meshDirty || (isSkinned && this._IsMeshBonesUpdate()))
                {
                    this._meshDirty = false;
                    this._UpdateMesh();
                }

                if (isSkinned)
                {
                    if (this._transformDirty)
                    {
                        this._transformDirty = false;
                        this._UpdateTransform(true);
                    }

                    return;
                }
            }

            if (this._transformDirty)
            {
                this._transformDirty = false;

                if (this._cachedFrameIndex < 0)
                {
                    var isCache = cacheFrameIndex >= 0;
                    this._UpdateGlobalTransformMatrix(isCache);

                    if (isCache && this._cachedFrameIndices != null)
                    {
                        this._cachedFrameIndex = this._cachedFrameIndices[cacheFrameIndex] = this._armature._armatureData.SetCacheFrame(this.globalTransformMatrix, this.global);
                    }
                }
                else
                {
                    this._armature._armatureData.GetCacheFrame(this.globalTransformMatrix, this.global, this._cachedFrameIndex);
                }

                this._UpdateTransform(false);
            }
        }

        /**
         * @private
         */
        public void UpdateTransformAndMatrix()
        {
            if (this._transformDirty)
            {
                this._transformDirty = false;
                this._UpdateGlobalTransformMatrix(false);
            }
        }

        /**
         * @private
         */
        internal void ReplaceDisplayData(DisplayData value, int displayIndex = -1)
        {
            if (displayIndex< 0)
            {
                if (this._displayIndex < 0)
                {
                    displayIndex = 0;
                }
                else
                {
                    displayIndex = this._displayIndex;
                }
            }

            if (this._displayDatas.Count <= displayIndex)
            {
                this._displayDatas.ResizeList(displayIndex + 1);

                for (int i = 0, l = this._displayDatas.Count; i < l; ++i)
                {
                    // Clean undefined.
                    this._displayDatas[i] = null;
                }
            }

            this._displayDatas [displayIndex] = value;
        }

        /**
         * 判断指定的点是否在插槽的自定义包围盒内。
         * @param x 点的水平坐标。（骨架内坐标系）
         * @param y 点的垂直坐标。（骨架内坐标系）
         * @param color 指定的包围盒颜色。 [0: 与所有包围盒进行判断, N: 仅当包围盒的颜色为 N 时才进行判断]
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public bool ContainsPoint(float x, float y)
        {
            if (this._boundingBoxData == null)
            {
                return false;
            }

            this.UpdateTransformAndMatrix();

            Slot._helpMatrix.CopyFrom(this.globalTransformMatrix);
            Slot._helpMatrix.Invert();
            Slot._helpMatrix.TransformPoint(x, y, Slot._helpPoint);

            return this._boundingBoxData.ContainsPoint(Slot._helpPoint.x, Slot._helpPoint.y);
        }

        /**
         * 判断指定的线段与插槽的自定义包围盒是否相交。
         * @param xA 线段起点的水平坐标。（骨架内坐标系）
         * @param yA 线段起点的垂直坐标。（骨架内坐标系）
         * @param xB 线段终点的水平坐标。（骨架内坐标系）
         * @param yB 线段终点的垂直坐标。（骨架内坐标系）
         * @param intersectionPointA 线段从起点到终点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param intersectionPointB 线段从终点到起点与包围盒相交的第一个交点。（骨架内坐标系）
         * @param normalRadians 碰撞点处包围盒切线的法线弧度。 [x: 第一个碰撞点处切线的法线弧度, y: 第二个碰撞点处切线的法线弧度]
         * @returns 相交的情况。 [-1: 不相交且线段在包围盒内, 0: 不相交, 1: 相交且有一个交点且终点在包围盒内, 2: 相交且有一个交点且起点在包围盒内, 3: 相交且有两个交点, N: 相交且有 N 个交点]
         * @version DragonBones 5.0
         * @language zh_CN
         */
        public int IntersectsSegment(float xA, float yA, float xB, float yB,
                                    Point intersectionPointA = null,
                                    Point intersectionPointB = null,
                                    Point normalRadians = null)
        {
            if (this._boundingBoxData == null)
            {
                return 0;
            }

            this.UpdateTransformAndMatrix();
            Slot._helpMatrix.CopyFrom(this.globalTransformMatrix);
            Slot._helpMatrix.Invert();
            Slot._helpMatrix.TransformPoint(xA, yA, Slot._helpPoint);
            xA = Slot._helpPoint.x;
            yA = Slot._helpPoint.y;
            Slot._helpMatrix.TransformPoint(xB, yB, Slot._helpPoint);
            xB = Slot._helpPoint.x;
            yB = Slot._helpPoint.y;

            var intersectionCount = this._boundingBoxData.IntersectsSegment(xA, yA, xB, yB, intersectionPointA, intersectionPointB, normalRadians);
            if (intersectionCount > 0)
            {
                if (intersectionCount == 1 || intersectionCount == 2)
                {
                    if (intersectionPointA != null)
                    {
                        this.globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);

                        if (intersectionPointB != null)
                        {
                            intersectionPointB.x = intersectionPointA.x;
                            intersectionPointB.y = intersectionPointA.y;
                        }
                    }
                    else if (intersectionPointB != null)
                    {
                        this.globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }
                else
                {
                    if (intersectionPointA != null)
                    {
                        this.globalTransformMatrix.TransformPoint(intersectionPointA.x, intersectionPointA.y, intersectionPointA);
                    }

                    if (intersectionPointB != null)
                    {
                        this.globalTransformMatrix.TransformPoint(intersectionPointB.x, intersectionPointB.y, intersectionPointB);
                    }
                }

                if (normalRadians != null)
                {
                    this.globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.x), (float)Math.Sin(normalRadians.x), Slot._helpPoint, true);
                    normalRadians.x = (float)Math.Atan2(Slot._helpPoint.y, Slot._helpPoint.x);

                    this.globalTransformMatrix.TransformPoint((float)Math.Cos(normalRadians.y), (float)Math.Sin(normalRadians.y), Slot._helpPoint, true);
                    normalRadians.y = (float)Math.Atan2(Slot._helpPoint.y, Slot._helpPoint.x);
                }
            }

            return intersectionCount;
        }

        /**
         * 在下一帧更新显示对象的状态。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public void InvalidUpdate()
        {
            this._displayDirty = true;
            this._transformDirty = true;
        }
        public bool visible
        {
            get { return this._visible; }
            set
            {
                if (this._visible == value)
                {
                    return;
                }

                this._visible = value;
                this._UpdateVisible();
            }
        }
        /**
         * 此时显示的显示对象在显示列表中的索引。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        public int displayIndex
        {
            get { return this._displayIndex; }
            set
            {
                if (this._SetDisplayIndex(value))
                {
                    this.Update(-1);
                }
            }
        }

        public string name
        {
            get { return this._slotData.name; }
        }

        /**
         * 包含显示对象或子骨架的显示列表。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public List<object> displayList
        {
            get { return new List<object>(_displayList.ToArray()); }
            set
            {
                var backupDisplayList = _displayList.ToArray(); // Copy.
                var disposeDisplayList = new List<object>();

                if (this._SetDisplayList(value))
                {
                    this.Update(-1);
                }

                // Release replaced displays.
                foreach (var eachDisplay in backupDisplayList)
                {
                    if ( eachDisplay != null &&
                        eachDisplay != this._rawDisplay &&
                        eachDisplay != this._meshDisplay &&
                        this._displayList.IndexOf(eachDisplay) < 0 &&
                        disposeDisplayList.IndexOf(eachDisplay) < 0)
                    {
                        disposeDisplayList.Add(eachDisplay);
                    }
                }

                foreach (var eachDisplay in disposeDisplayList)
                {
                    if (eachDisplay is Armature)
                    {
                        (eachDisplay as Armature).Dispose();
                    }
                    else
                    {
                        this._DisposeDisplay(eachDisplay);
                    }
                }
            }
        }
        public SlotData slotData
        {
            get { return this._slotData; }
        }

        public List<DisplayData> rawDisplayDatas
        {
            get { return this._rawDisplayDatas; }
            set
            {
                if (this._rawDisplayDatas == value)
                {
                    return;
                }

                this._displayDirty = true;
                this._rawDisplayDatas = value;

                if (this._rawDisplayDatas != null)
                {
                    this._displayDatas.ResizeList(this._rawDisplayDatas.Count);
                    for (int i = 0, l = this._displayDatas.Count; i < l; ++i)
                    {
                        this._displayDatas[i] = this._rawDisplayDatas[i];
                    }
                }
                else
                {
                    this._displayDatas.Clear();
                }
            }
        }
        /**
         * 插槽此时的自定义包围盒数据。
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public BoundingBoxData boundingBoxData
        {
            get { return this._boundingBoxData; }
        }
        /**
         * @private
         */
        public object rawDisplay
        {
            get { return this._rawDisplay; }
        }

        /**
         * @private
         */
        public object meshDisplay
        {
            get { return this._meshDisplay; }
        }
        /**
         * 此时显示的显示对象。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public object display
        {
            get { return this._display; }
            set
            {
                if (this._display == value)
                {
                    return;
                }

                var displayListLength = this._displayList.Count;
                if (this._displayIndex < 0 && displayListLength == 0)
                {  
                    // Emprty.
                    this._displayIndex = 0;
                }

                if (this._displayIndex < 0)
                {
                    return;
                }
                else
                {
                    var replaceDisplayList = this.displayList; // Copy.
                    if (displayListLength <= this._displayIndex)
                    {
                        replaceDisplayList.ResizeList(this._displayIndex + 1);
                    }

                    replaceDisplayList[this._displayIndex] = value;
                    this.displayList = replaceDisplayList;
                }
            }
        }
        /**
         * 此时显示的子骨架。
         * @see dragonBones.Armature
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public Armature childArmature
        {
            get { return this._childArmature; }
            set
            {
                if (this._childArmature == value)
                {
                    return;
                }

                this.display = value;
            }
        }
}
}
