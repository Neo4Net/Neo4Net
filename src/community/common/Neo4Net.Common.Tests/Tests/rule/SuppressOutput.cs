/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace Neo4Net.Test.rule
{
   using Description = org.junit.runner.Description;
   using Statement = org.junit.runners.model.Statement;
   using TestRule = org.junit.rules.TestRule;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static System.lineSeparator;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Arrays.asList;

   /// <summary>
   /// Suppresses outputs such as System.out, System.err and java.util.logging for example when running a test.
   /// It's also a <seealso cref="TestRule"/> which makes it fit in nicely in JUnit.
   ///
   /// The suppressing occurs visitor-style and if there's an exception in the code executed when being muted
   /// all the logging that was temporarily muted will be resent to the peers as if they weren't muted to begin with.
   /// </summary>
   public sealed class SuppressOutput : TestRule
   {
      private static readonly Suppressible _javaUtilLogging = _javaUtilLogging(new MemoryStream(), null);

      public static SuppressOutput Suppress(params Suppressible[] suppressibles)
      {
         return new SuppressOutput(suppressibles);
      }

      public static SuppressOutput SuppressAll()
      {
         return Suppress(System.out, System.Err, _javaUtilLogging);
      }

      public abstract class System : Suppressible
      {
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           out { PrintStream replace(java.io.PrintStream replacement) { java.io.PrintStream old = System.out; System.setOut(replacement); return old; } },
         //JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
         //           err { PrintStream replace(java.io.PrintStream replacement) { java.io.PrintStream old = System.err; System.setErr(replacement); return old; } };

         private static readonly IList<System> valueList = new List<System>();

         static System()
         {
            valueList.Add(@out);
            valueList.Add(err);
         }

         public enum InnerEnum
         {
            @out,
            err
         }

         public readonly InnerEnum innerEnumValue;
         private readonly string nameValue;
         private readonly int ordinalValue;
         private static int nextOrdinal = 0;

         private System(string name, InnerEnum innerEnum)
         {
            nameValue = name;
            ordinalValue = nextOrdinal++;
            innerEnumValue = innerEnum;
         }

         public Voice Suppress()
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.io.ByteArrayOutputStream buffer = new java.io.ByteArrayOutputStream();
            MemoryStream buffer = new MemoryStream();
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.io.PrintStream old = replace(new java.io.PrintStream(buffer));
            PrintStream old = Replace(new PrintStream(buffer));
            return new VoiceAnonymousInnerClass(this, buffer, old);
         }

         //JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
         //			  private static class VoiceAnonymousInnerClass extends Voice
         //		  {
         //			  private final System outerInstance;
         //
         //			  private System.IO.MemoryStream_Output buffer;
         //			  private java.io.PrintStream old;
         //
         //			  public VoiceAnonymousInnerClass(System outerInstance, System.IO.MemoryStream_Output buffer, java.io.PrintStream old)
         //			  {
         //				  base(outerInstance, buffer);
         //				  this.outerInstance = outerInstance;
         //				  this.buffer = buffer;
         //				  this.old = old;
         //			  }
         //
         //			  @@Override void restore(boolean failure) throws IOException
         //			  {
         //					replace(old).flush();
         //					if (failure)
         //					{
         //						 old.write(buffer.toByteArray());
         //					}
         //			  }
         //		  }

         internal abstract java.io.PrintStream replace(java.io.PrintStream replacement);

         public static IList<System> values()
         {
            return valueList;
         }

         public int ordinal()
         {
            return ordinalValue;
         }

         public override string ToString()
         {
            return nameValue;
         }

         public static System ValueOf(string name)
         {
            foreach (System enumInstance in System.valueList)
            {
               if (enumInstance.nameValue == name)
               {
                  return enumInstance;
               }
            }
            throw new System.ArgumentException(name);
         }
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public static Suppressible java_util_logging(final java.io.ByteArrayOutputStream redirectTo, java.util.logging.Level level)
      public static Suppressible JavaUtilLogging(MemoryStream redirectTo, Level level)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final java.util.logging.Handler replacement = redirectTo == null ? null : new java.util.logging.StreamHandler(redirectTo, new java.util.logging.SimpleFormatter());
         Handler replacement = redirectTo == null ? null : new StreamHandler(redirectTo, new SimpleFormatter());
         if (replacement != null && level != null)
         {
            replacement.Level = level;
         }
         return new SuppressibleAnonymousInnerClass(redirectTo, level, replacement);
      }

      private class SuppressibleAnonymousInnerClass : Suppressible
      {
         private MemoryStream _redirectTo;
         private Level _level;
         private Handler _replacement;

         public SuppressibleAnonymousInnerClass(MemoryStream redirectTo, Level level, Handler replacement)
         {
            this._redirectTo = redirectTo;
            this._level = level;
            this._replacement = replacement;
         }

         public Voice suppress()
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.logging.Logger logger = java.util.logging.LogManager.getLogManager().getLogger("");
            Logger logger = LogManager.LogManager.getLogger("");
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.logging.Level level = logger.getLevel();
            Level level = logger.Level;
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final java.util.logging.Handler[] handlers = logger.getHandlers();
            Handler[] handlers = logger.Handlers;
            foreach (Handler handler in handlers)
            {
               logger.removeHandler(handler);
            }
            if (_replacement != null)
            {
               logger.addHandler(_replacement);
               logger.Level = Level.ALL;
            }
            return new VoiceAnonymousInnerClass(this, _redirectTo, logger, level, handlers);
         }

         private class VoiceAnonymousInnerClass : Voice
         {
            private readonly SuppressibleAnonymousInnerClass _outerInstance;

            private Logger _logger;
            private Level _level;
            private Handler[] _handlers;

            public VoiceAnonymousInnerClass(SuppressibleAnonymousInnerClass outerInstance, MemoryStream redirectTo, Logger logger, Level level, Handler[] handlers) : base(outerInstance, redirectTo)
            {
               this.outerInstance = outerInstance;
               this._logger = logger;
               this._level = level;
               this._handlers = handlers;
            }

            internal override void restore(bool failure)
            {
               foreach (Handler handler in _handlers)
               {
                  _logger.addHandler(handler);
               }
               _logger.Level = _level;
               if (_outerInstance.replacement != null)
               {
                  _logger.removeHandler(_outerInstance.replacement);
               }
            }
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public <T> T call(java.util.concurrent.Callable<T> callable) throws Exception
      public T Call<T>(Callable<T> callable)
      {
         CaptureVoices();
         bool failure = true;
         try
         {
            T result = callable.call();
            failure = false;
            return result;
         }
         finally
         {
            ReleaseVoices(_voices, failure);
         }
      }

      private readonly Suppressible[] _suppressibles;
      private Voice[] _voices;

      private SuppressOutput(Suppressible[] suppressibles)
      {
         this._suppressibles = suppressibles;
      }

      public Voice[] AllVoices
      {
         get
         {
            return _voices;
         }
      }

      public Voice OutputVoice
      {
         get
         {
            return GetVoice(System.out );
         }
      }

      public Voice ErrorVoice
      {
         get
         {
            return GetVoice(System.Err);
         }
      }

      public Voice GetVoice(Suppressible suppressible)
      {
         foreach (Voice voice in _voices)
         {
            if (suppressible.Equals(voice.Suppressible))
            {
               return voice;
            }
         }
         return null;
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
      public override Statement Apply(Statement @base, Description description)
      {
         return new StatementAnonymousInnerClass(this, @base);
      }

      private class StatementAnonymousInnerClass : Statement
      {
         private readonly SuppressOutput _outerInstance;

         private Statement @base;

         public StatementAnonymousInnerClass(SuppressOutput outerInstance, Statement @base)
         {
            this.outerInstance = outerInstance;
            this.@base = @base;
         }

         //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
         //ORIGINAL LINE: public void evaluate() throws Throwable
         public override void evaluate()
         {
            outerInstance.CaptureVoices();
            bool failure = true;
            try
            {
               @base.evaluate();
               failure = false;
            }
            finally
            {
               _outerInstance.releaseVoices(_outerInstance.voices, failure);
            }
         }
      }

      public interface ISuppressible
      {
         Voice Suppress();
      }

      public abstract class Voice
      {
         //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
         internal Suppressible SuppressibleConflict;

         internal MemoryStream VoiceStream;

         public Voice(Suppressible suppressible, MemoryStream originalStream)
         {
            this.SuppressibleConflict = suppressible;
            this.VoiceStream = originalStream;
         }

         public virtual Suppressible Suppressible
         {
            get
            {
               return SuppressibleConflict;
            }
         }

         public virtual bool ContainsMessage(string message)
         {
            return VoiceStream.ToString().Contains(message);
         }

         /// <summary>
         /// Get each line written to this voice since it was suppressed </summary>
         public virtual IList<string> Lines()
         {
            return new IList<string> { ToString().Split(lineSeparator(), true) };
         }

         public override string ToString()
         {
            try
            {
               return VoiceStream.ToString(StandardCharsets.UTF_8.name());
            }
            catch (UnsupportedEncodingException e)
            {
               throw new Exception(e);
            }
         }

         //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
         //ORIGINAL LINE: abstract void restore(boolean failure) throws java.io.IOException;
         internal abstract void Restore(bool failure);
      }

      public void CaptureVoices()
      {
         Voice[] voices = new Voice[_suppressibles.Length];
         bool ok = false;
         try
         {
            for (int i = 0; i < voices.Length; i++)
            {
               voices[i] = _suppressibles[i].suppress();
            }
            ok = true;
         }
         finally
         {
            if (!ok)
            {
               ReleaseVoices(voices, false);
            }
         }
         this._voices = voices;
      }

      public void ReleaseVoices(bool failure)
      {
         ReleaseVoices(_voices, failure);
      }

      internal void ReleaseVoices(Voice[] voices, bool failure)
      {
         IList<Exception> failures = null;
         try
         {
            failures = new List<Exception>(voices.Length);
         }
         catch (Exception)
         {
            // nothing we can do...
         }
         foreach (Voice voice in voices)
         {
            if (voice != null)
            {
               try
               {
                  voice.Restore(failure);
               }
               catch (Exception exception)
               {
                  if (failures != null)
                  {
                     failures.Add(exception);
                  }
               }
            }
         }
         if (failures != null && failures.Count > 0)
         {
            foreach (Exception exception in failures)
            {
               Console.WriteLine(exception.ToString());
               Console.Write(exception.StackTrace);
            }
         }
      }
   }
}