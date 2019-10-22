using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Internal
{

	using ErrorState = Neo4Net.GraphDb.Events.ErrorState;
	using KernelEventHandler = Neo4Net.GraphDb.Events.KernelEventHandler;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Handle the collection of kernel event handlers, and fire events as needed.
	/// </summary>
	public class KernelEventHandlers : LifecycleAdapter
	{
		 private readonly IList<KernelEventHandler> _kernelEventHandlers = new CopyOnWriteArrayList<KernelEventHandler>();
		 private readonly Log _log;

		 public KernelEventHandlers( Log log )
		 {
			  this._log = log;
		 }

		 public override void Shutdown()
		 {
			  foreach ( KernelEventHandler kernelEventHandler in _kernelEventHandlers )
			  {
					kernelEventHandler.BeforeShutdown();
			  }
		 }

		 public virtual KernelEventHandler RegisterKernelEventHandler( KernelEventHandler handler )
		 {
			  if ( this._kernelEventHandlers.Contains( handler ) )
			  {
					return handler;
			  }

			  // Some algo for putting it in the right place
			  foreach ( KernelEventHandler registeredHandler in this._kernelEventHandlers )
			  {
					Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder order = handler.OrderComparedTo( registeredHandler );
					int index = this._kernelEventHandlers.IndexOf( registeredHandler );
					if ( order == Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.Before )
					{
						 this._kernelEventHandlers.Insert( index, handler );
						 return handler;
					}
					else if ( order == Neo4Net.GraphDb.Events.KernelEventHandler_ExecutionOrder.After )
					{
						 this._kernelEventHandlers.Insert( index + 1, handler );
						 return handler;
					}
			  }

			  this._kernelEventHandlers.Add( handler );
			  return handler;
		 }

		 public virtual KernelEventHandler UnregisterKernelEventHandler( KernelEventHandler handler )
		 {
			  if ( !_kernelEventHandlers.Remove( handler ) )
			  {
					throw new System.InvalidOperationException( handler + " isn't registered" );
			  }
			  return handler;
		 }

		 public virtual void KernelPanic( ErrorState error, Exception cause )
		 {
			  foreach ( KernelEventHandler handler in _kernelEventHandlers )
			  {
					try
					{
						 handler.KernelPanic( error );
					}
					catch ( Exception e )
					{
						 if ( cause != null )
						 {
							  e.addSuppressed( cause );
						 }
						 _log.error( "FATAL: Error while handling kernel panic.", e );
					}
			  }
		 }
	}

}