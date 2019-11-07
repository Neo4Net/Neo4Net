﻿/*
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

namespace Neo4Net.Kernel.Lifecycle
{
   using State = Neo4Net.Kernel.Lifecycle.SafeLifecycle.State;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertEquals;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertFalse;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertTrue;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.kernel.lifecycle.SafeLifecycle.State.HALT;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.kernel.lifecycle.SafeLifecycle.State.IDLE;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.kernel.lifecycle.SafeLifecycle.State.PRE;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.kernel.lifecycle.SafeLifecycle.State.RUN;

   internal class SafeLifecycleTest
   {
      private bool InstanceFieldsInitialized = false;

      public SafeLifecycleTest()
      {
         if (!InstanceFieldsInitialized)
         {
            InitializeInstanceFields();
            InstanceFieldsInitialized = true;
         }
      }

      private void InitializeInstanceFields()
      {
         _init = ILifecycle.init;
         _start = ILifecycle.start;
         _stop = ILifecycle.stop;
         _shutdown = ILifecycle.shutdown;
         _ops = new ThrowingConsumer[] { _init, _start, _stop, _shutdown };
      }

      private ThrowingConsumer<ILifecycle, Exception> _init;
      private ThrowingConsumer<ILifecycle, Exception> _start;
      private ThrowingConsumer<ILifecycle, Exception> _stop;
      private ThrowingConsumer<ILifecycle, Exception> _shutdown;

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @SuppressWarnings("unchecked") private Neo4Net.function.ThrowingConsumer<Lifecycle,Throwable>[] ops = new Neo4Net.function.ThrowingConsumer[]{init, start, stop, shutdown};
      private ThrowingConsumer<ILifecycle, Exception>[] _ops;

      private object[][] _onSuccess = new object[][]
      {
          new State[] { IDLE, null, null, HALT },
          new State[] { null, RUN, IDLE, HALT },
          new State[] { null, null, IDLE, null },
          new State[] { null, null, null, null }
      };

      private object[][] _onFailed = new object[][]
      {
          new State[] { PRE, null, null, HALT },
          new State[] { null, IDLE, IDLE, HALT },
          new State[] { null, null, IDLE, null },
          new State[] { null, null, null, null }
      };

      private bool?[][] _ignored = new bool?[][]
      {
          new bool?[] { false, false, false, true },
          new bool?[] { false, false, true, false },
          new bool?[] { false, false, false, false },
          new bool?[] { false, false, false, false }
      };

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldPerformSuccessfulTransitionsCorrectly() throws Throwable
             //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      internal virtual void ShouldPerformSuccessfulTransitionsCorrectly()
      {
         for (int state = 0; state < Enum.GetValues(typeof(State)).length; state++)
         {
            for (int op = 0; op < _ops.Length; op++)
            {
               MySafeAndSuccessfulLife sf = new MySafeAndSuccessfulLife(Enum.GetValues(typeof(State))[state]);
               bool caughtIllegalTransition = false;
               try
               {
                  _ops[op].accept(sf);
               }
               catch (System.InvalidOperationException)
               {
                  caughtIllegalTransition = true;
               }

               if (_onSuccess[state][op] == null)
               {
                  assertTrue(caughtIllegalTransition);
                 Assert.Equals(Enum.GetValues(typeof(State))[state], sf.State());
               }
               else
               {
                  assertFalse(caughtIllegalTransition);
                 Assert.Equals(_onSuccess[state][op], sf.State());
                  int expectedOpCode = _ignored[state][op] ? -1 : op;
                 Assert.Equals(expectedOpCode, sf.OpCode);
               }
            }
         }
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldPerformFailedTransitionsCorrectly() throws Throwable
             //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      internal virtual void ShouldPerformFailedTransitionsCorrectly()
      {
         for (int state = 0; state < Enum.GetValues(typeof(State)).length; state++)
         {
            for (int op = 0; op < _ops.Length; op++)
            {
               MyFailedLife sf = new MyFailedLife(Enum.GetValues(typeof(State))[state]);
               bool caughtIllegalTransition = false;
               bool failedOperation = false;
               try
               {
                  _ops[op].accept(sf);
               }
               catch (System.InvalidOperationException)
               {
                  caughtIllegalTransition = true;
               }
               catch (System.NotSupportedException)
               {
                  failedOperation = true;
               }

               if (_onFailed[state][op] == null)
               {
                  assertTrue(caughtIllegalTransition);
                 Assert.Equals(Enum.GetValues(typeof(State))[state], sf.State());
               }
               else
               {
                  assertFalse(caughtIllegalTransition);
                 Assert.Equals(_onFailed[state][op], sf.State());

                  if (_ignored[state][op].Value)
                  {
                    Assert.Equals(-1, sf.OpCode);
                     assertFalse(failedOperation);
                  }
                  else
                  {
                    Assert.Equals(op, sf.OpCode);
                     assertTrue(failedOperation);
                  }
               }
            }
         }
      }

      private class MySafeAndSuccessfulLife : SafeLifecycle
      {
         internal int OpCode;

         internal MySafeAndSuccessfulLife(State state) : base(state)
         {
            OpCode = -1;
         }

         public override void Init0()
         {
            Invoke(0);
         }

         public override void Start0()
         {
            Invoke(1);
         }

         public override void Stop0()
         {
            Invoke(2);
         }

         public override void Shutdown0()
         {
            Invoke(3);
         }

         internal virtual void Invoke(int opCode)
         {
            if (this.OpCode == -1)
            {
               this.OpCode = opCode;
            }
            else
            {
               throw new Exception("Double invocation");
            }
         }
      }

      private class MyFailedLife : SafeLifecycle
      {
         internal int OpCode;

         internal MyFailedLife(State state) : base(state)
         {
            OpCode = -1;
         }

         public override void Init0()
         {
            Invoke(0);
         }

         public override void Start0()
         {
            Invoke(1);
         }

         public override void Stop0()
         {
            Invoke(2);
         }

         public override void Shutdown0()
         {
            Invoke(3);
         }

         internal virtual void Invoke(int opCode)
         {
            if (this.OpCode == -1)
            {
               this.OpCode = opCode;
               throw new System.NotSupportedException("I made a bo-bo");
            }
            else
            {
               throw new Exception("Double invocation");
            }
         }
      }
   }
}