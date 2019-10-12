/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Lifecycle
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.SafeLifecycle.State.HALT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.SafeLifecycle.State.IDLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.SafeLifecycle.State.PRE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.lifecycle.SafeLifecycle.State.RUN;

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
	public abstract class SafeLifecycle : Lifecycle
	{
		 private State _state;

		 protected internal SafeLifecycle() : this(PRE)
		 {
		 }

		 internal SafeLifecycle( State state )
		 {
			  this._state = state;
		 }

		 /// <param name="expected"> The expected from state. </param>
		 /// <param name="to"> The to state. </param>
		 /// <param name="op"> The state transition operation. </param>
		 /// <param name="force"> Causes the state to be updated regardless of if the operation throws. </param>
		 /// <exception cref="Throwable"> Issues. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void transition(State expected, State to, Operation op, boolean force) throws Throwable
		 private void Transition( State expected, State to, Operation op, bool force )
		 {
			  if ( _state != expected )
			  {
					throw new System.InvalidOperationException( string.Format( "Expected {0} but was {1}", expected, _state ) );
			  }

			  if ( force )
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void init() throws Throwable
		 public override void Init()
		 {
			 lock ( this )
			 {
				  Transition( PRE, IDLE, this.init0, false );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void start() throws Throwable
		 public override void Start()
		 {
			 lock ( this )
			 {
				  Transition( IDLE, RUN, this.start0, false );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  if ( _state == IDLE )
				  {
						return;
				  }
				  Transition( RUN, IDLE, this.stop0, true );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  if ( _state == PRE )
				  {
						_state = HALT;
						return;
				  }
				  Transition( IDLE, HALT, this.shutdown0, true );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init0() throws Throwable
		 public virtual void Init0()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start0() throws Throwable
		 public virtual void Start0()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop0() throws Throwable
		 public virtual void Stop0()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown0() throws Throwable
		 public virtual void Shutdown0()
		 {
		 }

		 public virtual State State()
		 {
			  return _state;
		 }

		 protected internal enum State
		 {
			  Pre,
			  Idle,
			  Run,
			  Halt
		 }

		 internal interface Operation
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void run() throws Throwable;
			  void Run();
		 }
	}

}