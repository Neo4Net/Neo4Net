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
namespace Org.Neo4j.Kernel.impl.core
{

	using ErrorState = Org.Neo4j.Graphdb.@event.ErrorState;
	using KernelEventHandlers = Org.Neo4j.Kernel.@internal.KernelEventHandlers;

	public class DatabasePanicEventGenerator
	{
		 private readonly KernelEventHandlers _kernelEventHandlers;

		 public DatabasePanicEventGenerator( KernelEventHandlers kernelEventHandlers )
		 {
			  this._kernelEventHandlers = kernelEventHandlers;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void generateEvent(final org.neo4j.graphdb.event.ErrorState error, final Throwable cause)
		 public virtual void GenerateEvent( ErrorState error, Exception cause )
		 {
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  executor.execute( () => _kernelEventHandlers.kernelPanic(error, cause) );
			  executor.shutdown();
		 }
	}

}