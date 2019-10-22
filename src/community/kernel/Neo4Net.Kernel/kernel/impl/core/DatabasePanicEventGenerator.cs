using System;

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
namespace Neo4Net.Kernel.impl.core
{

	using ErrorState = Neo4Net.GraphDb.Events.ErrorState;
	using KernelEventHandlers = Neo4Net.Kernel.Internal.KernelEventHandlers;

	public class DatabasePanicEventGenerator
	{
		 private readonly KernelEventHandlers _kernelEventHandlers;

		 public DatabasePanicEventGenerator( KernelEventHandlers kernelEventHandlers )
		 {
			  this._kernelEventHandlers = kernelEventHandlers;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void generateEvent(final org.Neo4Net.graphdb.event.ErrorState error, final Throwable cause)
		 public virtual void GenerateEvent( ErrorState error, Exception cause )
		 {
			  ExecutorService executor = Executors.newSingleThreadExecutor();
			  executor.execute( () => _kernelEventHandlers.kernelPanic(error, cause) );
			  executor.shutdown();
		 }
	}

}