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

using System.Threading;

namespace Neo4Net.Test
{
   /// <summary>
   /// Controls two threads that would otherwise race and produce non-deterministic outcome.
   /// (ascii-art looks odd in source but lines up in fixed-size generated javadoc).
   /// <pre>
   ///          <seealso cref="Control.await() T1 await()"/>                   <seealso cref="Control.release() T1 release()"/>
   ///               |                              |
   /// -T1/T2--------|-T2-----------|-T1------------|-T1/T2------------------>
   ///                              |
   ///                        <seealso cref="reached() T2 reached()"/>
   /// </pre>
   /// </summary>
   public interface IBarrier
   {
      void Reached();
   }

   public static class Barrier_Fields
   {
      public static readonly Barrier None = null; //$!!$ tac () =>{};
   }

   public class Barrier_Control : IBarrier
   {
      internal readonly System.Threading.CountdownEvent _reachedConflict = new System.Threading.CountdownEvent(1);

      internal readonly System.Threading.CountdownEvent _released = new System.Threading.CountdownEvent(1);

      public void Reached()
      {
         try
         {
            _reachedConflict.Signal();
            _released.await();
         }
         catch (InterruptedException e)
         {
            throw new Exception(e);
         }
      }


      public virtual void Await()
      {
         _reachedConflict.await();
      }

      public virtual void AwaitUninterruptibly()
      {
         bool interrupted = false;
         try
         {
            while (true)
            {
               try
               {
                  Await();
                  return;
               }
               catch (InterruptedException)
               {
                  interrupted = true;
               }
            }
         }
         finally
         {
            if (interrupted)
            {
               Thread.CurrentThread.Interrupt();
            }
         }
      }

      public virtual void Release()
      {
         _released.Signal();
      }
   }
}