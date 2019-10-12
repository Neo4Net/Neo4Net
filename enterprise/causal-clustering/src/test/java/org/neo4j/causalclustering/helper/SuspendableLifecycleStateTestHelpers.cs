using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.helper
{
	using Org.Neo4j.Function;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

	internal class SuspendableLifecycleStateTestHelpers
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void setInitialState(StateAwareSuspendableLifeCycle lifeCycle, LifeCycleState state) throws Throwable
		 internal static void SetInitialState( StateAwareSuspendableLifeCycle lifeCycle, LifeCycleState state )
		 {
			  foreach ( LifeCycleState lifeCycleState in LifeCycleState.values() )
			  {
					if ( lifeCycleState.compareTo( state ) <= 0 )
					{
						 lifeCycleState.set( lifeCycle );
					}
			  }
		 }

		 internal sealed class LifeCycleState
		 {
			  public static readonly LifeCycleState Init = new LifeCycleState( "Init", InnerEnum.Init, Org.Neo4j.Kernel.Lifecycle.Lifecycle.init );
			  public static readonly LifeCycleState Start = new LifeCycleState( "Start", InnerEnum.Start, Org.Neo4j.Kernel.Lifecycle.Lifecycle.start );
			  public static readonly LifeCycleState Stop = new LifeCycleState( "Stop", InnerEnum.Stop, Org.Neo4j.Kernel.Lifecycle.Lifecycle.stop );
			  public static readonly LifeCycleState Shutdown = new LifeCycleState( "Shutdown", InnerEnum.Shutdown, Org.Neo4j.Kernel.Lifecycle.Lifecycle.shutdown );

			  private static readonly IList<LifeCycleState> valueList = new List<LifeCycleState>();

			  static LifeCycleState()
			  {
				  valueList.Add( Init );
				  valueList.Add( Start );
				  valueList.Add( Stop );
				  valueList.Add( Shutdown );
			  }

			  public enum InnerEnum
			  {
				  Init,
				  Start,
				  Stop,
				  Shutdown
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal readonly Org.Neo4j.Function.ThrowingConsumer<Org.Neo4j.Kernel.Lifecycle.Lifecycle, Exception> operation;

			  internal LifeCycleState( string name, InnerEnum innerEnum, Org.Neo4j.Function.ThrowingConsumer<Org.Neo4j.Kernel.Lifecycle.Lifecycle, Exception> operation )
			  {
					this._operation = operation;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void set(org.neo4j.kernel.lifecycle.Lifecycle lifecycle) throws Throwable
			  internal void Set( Org.Neo4j.Kernel.Lifecycle.Lifecycle lifecycle )
			  {
					_operation.accept( lifecycle );
			  }

			 public static IList<LifeCycleState> values()
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

			 public static LifeCycleState valueOf( string name )
			 {
				 foreach ( LifeCycleState enumInstance in LifeCycleState.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal sealed class SuspendedState
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           Untouched(suspendable -> {}),
			  public static readonly SuspendedState Enabled = new SuspendedState( "Enabled", InnerEnum.Enabled, Suspendable.enable );
			  public static readonly SuspendedState Disabled = new SuspendedState( "Disabled", InnerEnum.Disabled, Suspendable.disable );

			  private static readonly IList<SuspendedState> valueList = new List<SuspendedState>();

			  static SuspendedState()
			  {
				  valueList.Add( Untouched );
				  valueList.Add( Enabled );
				  valueList.Add( Disabled );
			  }

			  public enum InnerEnum
			  {
				  Untouched,
				  Enabled,
				  Disabled
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal readonly Org.Neo4j.Function.ThrowingConsumer<Suspendable, Exception> consumer;

			  internal SuspendedState( string name, InnerEnum innerEnum, Org.Neo4j.Function.ThrowingConsumer<Suspendable, Exception> consumer )
			  {
					this._consumer = consumer;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void set(Suspendable suspendable) throws Throwable
			  internal void Set( Suspendable suspendable )
			  {
					_consumer.accept( suspendable );
			  }

			 public static IList<SuspendedState> values()
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

			 public static SuspendedState valueOf( string name )
			 {
				 foreach ( SuspendedState enumInstance in SuspendedState.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}