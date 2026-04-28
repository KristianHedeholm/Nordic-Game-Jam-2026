using System;
using System.Runtime.InteropServices;
using RawPowerLabs.DynamicAI.Native;

namespace RawPowerLabs.DynamicAI
{
    public unsafe class Context : SafeHandle
    {
        public Context() : base((IntPtr)ResourceHandle.InvalidHandle, true)
        {
            SetHandle((IntPtr)CoreInterface.Instance->Context.CreateContext());
        }

        public override bool IsInvalid => (int)handle == ResourceHandle.InvalidHandle;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid) return true;

            CoreInterface.Instance->Context.DestroyContext((int)handle);

            // Clear the handle to avoid double releases.
            SetHandle((IntPtr)ResourceHandle.InvalidHandle);

            return true;
        }

        public TextModule? CreateTextModule(
            TextModuleParameters parameters,
            string ggufPath,
            string templatePath)
        {
            var ggufPathBytes     = NativeUtils.EncodeUtf8(ggufPath);
            var templatePathBytes = NativeUtils.EncodeUtf8(templatePath);

            fixed (byte* pGgufPath     = ggufPathBytes)
            fixed (byte* pTemplatePath = templatePathBytes)
            {
                var moduleHandle = CoreInterface.Instance->LocalTextModule.Create(
                    (int)handle,
                    ref parameters,
                    pGgufPath,
                    pTemplatePath
                );
                return moduleHandle == ResourceHandle.InvalidHandle ? null : new TextModule(this, moduleHandle);
            }
        }

        public CloudTextModule? CreateCloudTextModule(CloudTextModuleOptions options)
        {
            var aifactoryUrl     = NativeUtils.EncodeUtf8(options.AifactoryUrl);
            var designmoduleUrl  = NativeUtils.EncodeUtf8(options.DesignmoduleUrl);
            var zitadelUrl       = NativeUtils.EncodeUtf8(options.ZitadelUrl);
            var zitadelProjectId = NativeUtils.EncodeUtf8(options.ZitadelProjectId);
            var projectId        = NativeUtils.EncodeUtf8(options.ProjectId);
            var clientId         = NativeUtils.EncodeUtf8(options.ClientId);
            var clientSecret     = NativeUtils.EncodeUtf8(options.ClientSecret);
            var model            = NativeUtils.EncodeUtf8(options.Model);

            fixed (byte* pAifactoryUrl     = aifactoryUrl)
            fixed (byte* pDesignmoduleUrl  = designmoduleUrl)
            fixed (byte* pZitadelUrl       = zitadelUrl)
            fixed (byte* pZitadelProjectId = zitadelProjectId)
            fixed (byte* pProjectId        = projectId)
            fixed (byte* pClientId         = clientId)
            fixed (byte* pClientSecret     = clientSecret)
            fixed (byte* pModel            = model)
            {
                var parameters = new CloudTextModuleParameters();
                parameters.AifactoryUrl     = pAifactoryUrl;
                parameters.DesignmoduleUrl  = pDesignmoduleUrl;
                parameters.ZitadelUrl       = pZitadelUrl;
                parameters.ZitadelProjectId = pZitadelProjectId;
                parameters.ProjectId        = pProjectId;
                parameters.ClientId         = pClientId;
                parameters.ClientSecret     = pClientSecret;
                parameters.Model            = pModel;

                var moduleHandle = CoreInterface.Instance->CloudTextModule.Create((int)handle, ref parameters);
                return moduleHandle == ResourceHandle.InvalidHandle ? null : new CloudTextModule(this, moduleHandle);
            }
        }
    }
}
