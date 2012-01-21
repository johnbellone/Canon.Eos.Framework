using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Canon.Eos.Framework.Internal;

namespace Canon.Eos.Framework
{
    public class ImageTransferedEventArgs : EventArgs
    {
        private System.Drawing.Image _image;

        public System.Drawing.Image Image
        {
            get { return _image; }
        }

        public ImageTransferedEventArgs(System.Drawing.Image image)
        {
            _image = image;
        }
    }

    partial class EosCamera
    {
        private void OnObjectEventVolumeInfoChanged(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventVolumeUpdateItems(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventFolderUpdateItems(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventDirItemCreated(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventDirItemRemoved(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventDirItemInfoChanged(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventDirItemContentChanged(IntPtr sender, IntPtr context)
        {
        }

        private void CreateBitmapFromStream(IntPtr stream, Int32 size)
        {
            Debug.WriteLine("Canon.Eos.Framework.EosCamera.CreateBitmapFromStream");

            var image = IntPtr.Zero;
            try
            {
                EosAssert.NotOk(Edsdk.EdsGetPointer(stream, out image), "Failed to get memory pointer");

                var buffer = new byte[size];
                Marshal.Copy(image, buffer, 0, size);
                var ms = new MemoryStream(buffer);
                var bitmap = new System.Drawing.Bitmap(ms);

                if (ImageTransfered != null)
                {
                    ImageTransferedEventArgs args = new ImageTransferedEventArgs(bitmap);
                    ImageTransfered(this, args);
                }
            }
            catch (EosException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EosException(-1, "Unexpected exception while marshaling.", ex);
            }
            finally
            {
                if (image != IntPtr.Zero)
                    Edsdk.EdsRelease(image);
            }
        }
        
        private void OnObjectEventDirItemRequestTransfer(IntPtr sender, IntPtr context)
        {
            Debug.WriteLine("Canon.Eos.Framework.EosCamera.OnObjectEventDirItemRequestTransfer");

            var stream = IntPtr.Zero;
            try
            {
                Edsdk.EdsDirectoryItemInfo directoryItemInfo;
                EosAssert.NotOk(Edsdk.EdsGetDirectoryItemInfo(sender, out directoryItemInfo), "Failed to get directory item info.");
                
                var location = Path.Combine(_picturePath ?? Environment.CurrentDirectory, directoryItemInfo.szFileName);

                EosAssert.NotOk(Edsdk.EdsCreateMemoryStream(directoryItemInfo.Size, out stream), "Failed to create memory stream");                
                EosAssert.NotOk(Edsdk.EdsDownload(sender, directoryItemInfo.Size, stream), "Failed to create file stream");
                EosAssert.NotOk(Edsdk.EdsDownloadComplete(sender), "Failed to complete download");

                CreateBitmapFromStream(stream, Convert.ToInt32(directoryItemInfo.Size));
            }
            catch (EosException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EosException(-1, "Unexpected exception while downloading.", ex);
            }
            finally
            {
                Edsdk.EdsRelease(sender);
                if (stream != IntPtr.Zero)
                    Edsdk.EdsRelease(stream);
            }
        }
        
        private void OnObjectEventDirItemRequestTransferDt(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventDirItemCancelTransferDt(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventVolumeAdded(IntPtr sender, IntPtr context)
        {
        }
        
        private void OnObjectEventVolumeRemoved(IntPtr sender, IntPtr context)
        {
        }

        private uint HandleObjectEvent(uint objectEvent, IntPtr sender, IntPtr context)
        {
            EosFramework.LogInstance.Debug("HandleObjectEvent fired: " + objectEvent);
            switch (objectEvent)
            {
                case Edsdk.ObjectEvent_VolumeInfoChanged:
                    this.OnObjectEventVolumeInfoChanged(sender, context);
                    break;
                case Edsdk.ObjectEvent_VolumeUpdateItems:
                    this.OnObjectEventVolumeUpdateItems(sender, context);
                    break;
                case Edsdk.ObjectEvent_FolderUpdateItems:
                    this.OnObjectEventFolderUpdateItems(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemCreated:
                    this.OnObjectEventDirItemCreated(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemRemoved:
                    this.OnObjectEventDirItemRemoved(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemInfoChanged:
                    this.OnObjectEventDirItemInfoChanged(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemContentChanged:
                    this.OnObjectEventDirItemContentChanged(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemRequestTransfer:
                    this.OnObjectEventDirItemRequestTransfer(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemRequestTransferDT:
                    this.OnObjectEventDirItemRequestTransferDt(sender, context);
                    break;
                case Edsdk.ObjectEvent_DirItemCancelTransferDT:
                    this.OnObjectEventDirItemCancelTransferDt(sender, context);
                    break;
                case Edsdk.ObjectEvent_VolumeAdded:
                    this.OnObjectEventVolumeAdded(sender, context);
                    break;
                case Edsdk.ObjectEvent_VolumeRemoved:
                    this.OnObjectEventVolumeRemoved(sender, context);
                    break;
            }

            return Edsdk.EDS_ERR_OK;
        }
    }
}
