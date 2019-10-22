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

namespace Neo4Net.Kernel.Lifecycle
{
 

   /// <summary>
   /// A safer lifecycle adapter with strict semantics and as
   /// a result simpler error handling and reasoning. If the
   /// semantics of this class is not consistent with what you
   /// are after, then it is probably better to create a new
   /// adapter with the specific semantics required.
   /// <pre>
   /// Guide
   ///
   ///  *: No-op    (will not invoke operation)
   ///  -: Error    (will throw IllegalStateException)
   ///
   ///  P: PRE      ("pre init")
   ///  I: IDLE     ("initialized" or "stopped")
   ///  R: RUN      ("started")
   ///  H: HALT     ("shutdown")
   /// </pre>
   /// A successful operation is an operation not throwing an exception.
   /// <para>
   /// End states on a successful operation.
   /// <pre>
   ///  ---------------------------------------------------
   /// | FROM \ op |  init()  start()  stop()  shutdown()  |
   ///  ---------------------------------------------------
   /// | PRE       |   IDLE      -       -       HALT(*)   |
   /// | IDLE      |    -       RUN    IDLE(*)    HALT     |
   /// | RUN       |    -        -      IDLE       -       |
   /// | HALT      |    -        -       -         -       |
   ///  ---------------------------------------------------
   /// </pre>
   /// End states on a failed operation.
   /// <pre>
   ///  ---------------------------------------------------
   /// | FROM \ op |  init()  start()  stop()  shutdown()  |
   ///  ---------------------------------------------------
   /// | PRE       |   PRE       -       -       HALT(*)   |
   /// | IDLE      |    -       IDLE   IDLE(*)    HALT     |
   /// | RUN       |    -        -      IDLE       -       |
   /// | HALT      |    -        -       -         -       |
   ///  ---------------------------------------------------
   /// </pre>
   /// A few notes:
   /// <ul>
   /// <li>will not invoke stop0() if start0() wasn't successful</li>
   /// <li>will not invoke shutdown0() if init0() wasn't successful</li>
   /// </ul>
   /// The expectation with regards to error handling and cleanup is that
   /// an unclean start() is cleaned up by the start0() method and thus
   /// the component is left in IDLE. The same goes for issues happening
   /// during init0(), which leaves the component in PRE.
   /// </para>
   /// <para>
   /// Because of the way that <seealso cref="LifeSupport"/> operates today, this
   /// class will ignore stop() calls made while in IDLE. Similarly, calls
   /// to shutdown() will be ignored while in PRE. This allows this class
   /// to be managed by a <seealso cref="LifeSupport"/> without throwing
   /// <seealso cref="System.InvalidOperationException"/> on those state transitions, which
   /// otherwise would have been disallowed and handled in the same way
   /// as other illegal state transitions.
   /// </para>
   /// <para>
   /// This adapter will not allow a shutdown lifecycle to be reinitialized
   /// and started again.
   /// </para>
   /// </summary>
   public abstract class SafeLifecycle : ILifecycle
   {
      private StateType _state;

      protected internal SafeLifecycle() : this(StateType.Pre)
      {
      }

      internal SafeLifecycle(StateType state)
      {
         this._state = state;
      }

      /// <param name="expected"> The expected from state. </param>
      /// <param name="to"> The to state. </param>
      /// <param name="op"> The state transition operation. </param>
      /// <param name="force"> Causes the state to be updated regardless of if the operation throws. </param>
      /// <exception cref="Throwable"> Issues. </exception>
      
      private void Transition(StateType expected, StateType to, IOperation op, bool force)
      {
         if (_state != expected)
         {
            throw new System.InvalidOperationException(string.Format("Expected {0} but was {1}", expected, _state));
         }

         if (force)
         {
            _state = to;
            op.Run();
         }
         else
         {
            op.Run();
            _state = to;
         }
      }

      public void Init()
      {
         lock (this)
         {
            Transition(StateType.Pre, StateType.Idle, this.Init0, false);
         }
      }


      public void Start()
      {
         lock (this)
         {
            Transition(StateType.Idle, StateType.Run, this.Start0, false);
         }
      }

   
      public void Stop()
      {
         lock (this)
         {
            if (_state == Idle)
            {
               return;
            }
            Transition(Run, IDLE, this.Stop0, true);
         }
      }


      public void Shutdown()
      {
         lock (this)
         {
            if (_state == PRE)
            {
               _state = HALT;
               return;
            }
            Transition(IDLE, HALT, this.Shutdown0, true);
         }
      }


      public virtual void Init0()
      {
      }


      public virtual void Start0()
      {
      }

  
      public virtual void Stop0()
      {
      }


      public virtual void Shutdown0()
      {
      }

      public virtual StateType State()
      {
         return _state;
      }

      public internal enum StateType
      {
         Pre,
         Idle,
         Run,
         Halt
      }

      internal interface IOperation
      {

         void Run();
      }
   }
}