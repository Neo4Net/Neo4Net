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

namespace Neo4Net.GraphDb.Events
{
   /// <summary>
   /// Event handler interface for Neo4Net Kernel life cycle events.
   ///
   /// </summary>
   public interface IKernelEventHandler
   {
      /// <summary>
      /// This method is invoked during the shutdown process of a Neo4Net Graph
      /// Database. It is invoked while the <seealso cref="GraphDatabaseService"/> is still
      /// in an operating state, after the processing of this event has terminated
      /// the Neo4Net Graph Database will terminate. This event can be used to shut
      /// down other services that depend on the <seealso cref="GraphDatabaseService"/>.
      /// </summary>
      void BeforeShutdown();

      /// <summary>
      /// This is invoked when the Neo4Net Graph Database enters a state from which
      /// it cannot continue.
      /// </summary>
      /// <param name="error"> an object describing the state that the
      ///            <seealso cref="GraphDatabaseService"/> failed to recover from. </param>
      void KernelPanic(ErrorState error);

      /// <summary>
      /// Returns the resource associated with this event handler, or {@code null}
      /// if no specific resource is associated with this handler or if it isn't
      /// desirable to expose it. It can be used to aid in the decision process
      /// of in which order to execute the handlers, see
      /// <seealso cref="orderComparedTo(IKernelEventHandler)"/>.
      /// </summary>
      /// <returns> the resource associated to this event handler, or {@code null}. </returns>
      object Resource { get; }

      /// <summary>
      /// Gives a hint about when to execute this event handler, compared to other
      /// handlers. If this handler must be executed before {@code other} then
      /// <seealso cref="ExecutionOrder.BEFORE"/> should be returned. If this handler must be
      /// executed after {@code other} then <seealso cref="ExecutionOrder.AFTER"/> should be
      /// returned. If it doesn't matter <seealso cref="ExecutionOrder.DOESNT_MATTER"/>
      /// should be returned.
      /// </summary>
      /// <param name="other"> the other event handler to compare to. </param>
      /// <returns> the execution order compared to {@code other}. </returns>
      KernelEventHandler_ExecutionOrder OrderComparedTo(IKernelEventHandler other);

      /// <summary>
      /// Represents the order of execution between two event handlers, if one
      /// handler should be executed <seealso cref="ExecutionOrder.BEFORE"/>,
      /// <seealso cref="ExecutionOrder.AFTER"/> another handler, or if it
      /// <seealso cref="ExecutionOrder.DOESNT_MATTER"/>.
      ///
      /// @author mattias
      ///
      /// </summary>
   }

   public enum KernelEventHandler_ExecutionOrder
   {
      /// <summary>
      /// Says that the event handler must be executed before the compared
      /// event handler.
      /// </summary>
      Before,

      /// <summary>
      /// Says that the event handler must be executed after the compared
      /// event handler.
      /// </summary>
      After,

      /// <summary>
      /// Says that it doesn't matter in which order the event handler is
      /// executed in comparison to another event handler.
      /// </summary>
      DoesntMatter
   }
}