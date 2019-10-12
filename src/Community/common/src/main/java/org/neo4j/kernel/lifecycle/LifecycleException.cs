using System;
using System.Text;

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
namespace Neo4Net.Kernel.Lifecycle
{
	/// <summary>
	/// This exception is thrown by LifeSupport if a lifecycle transition fails. If many exceptions occur
	/// they will be chained through the cause exception mechanism.
	/// </summary>
	public class LifecycleException : Exception
	{

		 public LifecycleException( object instance, LifecycleStatus from, LifecycleStatus to, Exception cause ) : base( HumanReadableMessage( instance, from, to, cause ), cause )
		 {
		 }

		 public LifecycleException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 private static string HumanReadableMessage( object instance, LifecycleStatus from, LifecycleStatus to, Exception cause )
		 {
			  string instanceStr = instance.ToString();
			  StringBuilder message = new StringBuilder();
			  switch ( to )
			  {
					case Neo4Net.Kernel.Lifecycle.LifecycleStatus.Stopped:
						 if ( from == LifecycleStatus.None )
						 {
							  message.Append( "Component '" ).Append( instanceStr ).Append( "' failed to initialize" );
						 }
						 else if ( from == LifecycleStatus.Started )
						 {
							  message.Append( "Component '" ).Append( instanceStr ).Append( "' failed to stop" );
						 }
						 break;
					case Neo4Net.Kernel.Lifecycle.LifecycleStatus.Started:
						 if ( from == LifecycleStatus.Stopped )
						 {
							  message.Append( "Component '" ).Append( instanceStr ).Append( "' was successfully initialized, but failed to start" );
						 }
						 break;
					case Neo4Net.Kernel.Lifecycle.LifecycleStatus.Shutdown:
						 message.Append( "Component '" ).Append( instanceStr ).Append( "' failed to shut down" );
						 break;
					default:
						 break;
			  }
			  if ( message.Length == 0 )
			  {
					message.Append( "Component '" ).Append( instanceStr ).Append( "' failed to transition from " ).Append( from.name().ToLower() ).Append(" to ").Append(to.name().ToLower());
			  }
			  message.Append( '.' );
			  if ( cause != null )
			  {
					Exception root = RootCause( cause );
					message.Append( " Please see the attached cause exception \"" ).Append( root.Message ).Append( '"' );
					if ( root.InnerException != null )
					{
						 message.Append( " (root cause cycle detected)" );
					}
					message.Append( '.' );
			  }

			  return message.ToString();
		 }

		 private static Exception RootCause( Exception cause )
		 {
			  int i = 0; // Guard against infinite self-cause exception-loops.
			  while ( cause.InnerException != null && i++ < 100 )
			  {
					cause = cause.InnerException;
			  }
			  return cause;
		 }
	}

}