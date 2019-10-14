/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.cluster.statemachine
{

	/// <summary>
	/// Handle for dynamic proxies that are backed by a <seealso cref="StateMachine"/>.
	/// Delegates calls to a <seealso cref="StateMachineProxyFactory"/>, which in turn
	/// will call the <seealso cref="StateMachine"/>.
	/// </summary>
	public class StateMachineProxyHandler : InvocationHandler
	{
		 private StateMachineProxyFactory _stateMachineProxyFactory;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private StateMachine<?,?> stateMachine;
		 private StateMachine<object, ?> _stateMachine;

		 public StateMachineProxyHandler<T1>( StateMachineProxyFactory stateMachineProxyFactory, StateMachine<T1> stateMachine )
		 {
			  this._stateMachineProxyFactory = stateMachineProxyFactory;
			  this._stateMachine = stateMachine;
		 }

		 public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
		 {
			  // Delegate call to factory, which will translate method call into state machine invocation
			  return _stateMachineProxyFactory.invoke( _stateMachine, method, args == null ? null : ( args.Length > 1 ? args : args[0] ) );
		 }

		 public virtual StateMachineProxyFactory StateMachineProxyFactory
		 {
			 get
			 {
				  return _stateMachineProxyFactory;
			 }
		 }
	}

}