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

namespace Neo4Net.Kernel.Lifecycle
{
   public class Lifecycles
   {
      private Lifecycles()
      { // No instances allowed or even necessary
      }

      //JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
      //ORIGINAL LINE: public static Lifecycle multiple(final Iterable<? extends Lifecycle> lifecycles)
      public static ILifecycle Multiple<T1>(IEnumerable<T1> lifecycles) where T1 : ILifecycle
      {
         return new CombinedLifecycle<T1>(lifecycles);
      }

      public static ILifecycle Multiple(params ILifecycle[] lifecycles)
      {
         return Multiple(Array.asList(lifecycles));
      }

      private class CombinedLifecycle<T> : ILifecycle where T : ILifecycle
      {
         internal readonly IEnumerable<ILifecycle> _lifecycles;

         internal CombinedLifecycle<T> ( IEnumerable<T> lifecycles ) where T : ILifecycle
			  {
				   _lifecycles = lifecycles;
			  }


      public void Init()
      {
         foreach (ILifecycle lifecycle in _lifecycles)
         {
            lifecycle.Init();
         }
      }

      public void Start()
      {
         foreach (ILifecycle lifecycle in _lifecycles)
         {
            lifecycle.Start();
         }
      }


      public  void Stop()
      {
         foreach (ILifecycle lifecycle in _lifecycles)
         {
            lifecycle.Stop();
         }
      }

      public void Shutdown()
      {
         foreach (ILifecycle lifecycle in _lifecycles)
         {
            lifecycle.Shutdown();
         }
      }
   }
}
}