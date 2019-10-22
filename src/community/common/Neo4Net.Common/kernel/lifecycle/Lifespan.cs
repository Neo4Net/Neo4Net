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

using System;

namespace Neo4Net.Kernel.Lifecycle
{
   /// <summary>
   /// Convenient use of a <seealso cref="LifeSupport"/>, effectively making one or more <seealso cref="ILifecycle"/> look and feel
   /// like one <seealso cref="IDisposable"/>.
   /// </summary>
   public class Lifespan : IDisposable
   {
      private readonly LifeSupport _life = new LifeSupport();
      private bool disposed = false; // to detect redundant calls

      public Lifespan(params ILifecycle[] subjects)
      {
         foreach (ILifecycle subject in subjects)
         {
            _life.Add(subject);
         }
         _life.Start();
      }

      public virtual T Add<T>(T subject) where T : ILifecycle
      {
         return _life.Add(subject);
      }

      public  void Close()
      {
         _life.Shutdown();
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposed)
         {
            if (disposing)
            {
               // Dispose managed resources.
               Close();
            }

            // There are no unmanaged resources to release, but
            // if we add them, they need to be released here.
         }
         disposed = true;

         // If it is available, make the call to the
         // base class's Dispose(Boolean) method
         //$!!$base.Dispose(disposing);
      }



   }
}