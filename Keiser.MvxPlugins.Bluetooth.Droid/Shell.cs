namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using System;

    public static class Shell
    {
        public static void Command(string command)
        {
            // Requires root access to run
            try
            {
                Java.Lang.Process process = Java.Lang.Runtime.GetRuntime().Exec("su");
                Java.IO.DataOutputStream outputStream = new Java.IO.DataOutputStream(process.OutputStream);

                outputStream.WriteBytes(command + "\n");
                outputStream.WriteBytes("exit\n");
                outputStream.Flush();
                try
                {
                    process.WaitFor();
                }
#if DEBUG
                catch (Exception e)
                {

                    Trace.Error(e.ToString());
                }
#else
                catch { }
#endif
                outputStream.Close();
            }
#if DEBUG
            catch (Exception e)
            {

                Trace.Error(e.ToString());
            }
#else
            catch { }
#endif
        }
    }
}