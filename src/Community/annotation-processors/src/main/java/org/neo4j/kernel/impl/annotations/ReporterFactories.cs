using System;

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
namespace Neo4Net.Kernel.Impl.Annotations
{

	public class ReporterFactories
	{
		 private static readonly InvocationHandler _throwingHandler = new ThrowingInvocationHandler();
		 private static readonly InvocationHandler _noopHandler = ( proxy, method, args ) => null;

		 public static ReporterFactory ThrowingReporterFactory()
		 {
			  return new ReporterFactory( _throwingHandler );
		 }

		 public static ReporterFactory NoopReporterFactory()
		 {
			  return new ReporterFactory( _noopHandler );
		 }

		 private class ThrowingInvocationHandler : InvocationHandler
		 {
			  public override object Invoke( object proxy, System.Reflection.MethodInfo method, object[] args )
			  {
					throw new Exception( DocumentedUtils.ExtractFormattedMessage( method, args ) );
			  }
		 }
	}

}