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
   public interface Barrier
   {
      void Reached();
   }

   public static class Barrier_Fields
   {
      public static readonly Barrier None = () =>
      {
      };
   }

   public class Barrier_Control : Barrier
   {
      //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
      internal readonly System.Threading.CountdownEvent ReachedConflict = new System.Threading.CountdownEvent(1);

      internal readonly System.Threading.CountdownEvent Released = new System.Threading.CountdownEvent(1);

      public override void Reached()
      {
         try
         {
            ReachedConflict.Signal();
            Released.await();
         }
         catch (InterruptedException e)
         {
            throw new Exception(e);
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public void await() throws InterruptedException
      public virtual void Await()
      {
         ReachedConflict.await();
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
         Released.Signal();
      }
   }
}