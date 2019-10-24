using System;
using System.Collections.Generic;
using System.Threading;

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

namespace Neo4Net.Kernel.Api.Internal.Helpers
{
   using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
   using Registers = Neo4Net.Register.Registers;

   public class Indexes
   {
      /// <summary>
      /// For each index, await a resampling event unless it has zero pending updates.
      /// </summary>
      /// <param name="schemaRead"> backing schema read </param>
      /// <param name="timeout"> timeout in seconds. If this limit is passed, a TimeoutException is thrown. </param>
      /// <exception cref="TimeoutException"> if all indexes are not resampled within the timeout. </exception>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: public static void awaitResampling(org.Neo4Net.Kernel.Api.Internal.SchemaRead schemaRead, long timeout) throws java.util.concurrent.TimeoutException
      public static void AwaitResampling(ISchemaRead schemaRead, long timeout)
      {
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final java.util.Iterator<org.Neo4Net.Kernel.Api.Internal.IndexReference> indexes = schemaRead.indexesGetAll();
         IEnumerator<IIndexReference> indexes = schemaRead.IndexesGetAll();
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final org.Neo4Net.register.Register_DoubleLongRegister register = org.Neo4Net.register.Registers.newDoubleLongRegister();
         Neo4Net.Register.Register_DoubleLongRegister register = Registers.newDoubleLongRegister();
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final long t0 = System.currentTimeMillis();
         long t0 = DateTimeHelper.CurrentUnixTimeMillis();
         //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
         //ORIGINAL LINE: final long timeoutMillis = 1000 * timeout;
         long timeoutMillis = 1000 * timeout;

         while (indexes.MoveNext())
         {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.IndexReference index = indexes.Current;
            IIndexReference index = indexes.Current;
            try
            {
               long readUpdates = readUpdates(index, schemaRead, register);
               long updateCount = readUpdates;
               bool hasTimedOut = false;

               while (updateCount > 0 && updateCount <= readUpdates && !hasTimedOut)
               {
                  Thread.Sleep(10);
                  hasTimedOut = DateTimeHelper.CurrentUnixTimeMillis() - t0 >= timeoutMillis;
                  updateCount = Math.Max(updateCount, readUpdates);
                  readUpdates = readUpdates(index, schemaRead, register);
               }

               if (hasTimedOut)
               {
                  throw new TimeoutException(string.Format("Indexes were not resampled within {0} {1}", timeout, TimeUnit.SECONDS));
               }
            }
            catch (InterruptedException e)
            {
               Thread.CurrentThread.Interrupt();
               throw new Exception(e);
            }
            catch (IndexNotFoundKernelException e)
            {
               throw new ConcurrentModificationException("Index was dropped while awaiting resampling", e);
            }
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: private static long readUpdates(org.Neo4Net.Kernel.Api.Internal.IndexReference index, org.Neo4Net.Kernel.Api.Internal.SchemaRead schemaRead, org.Neo4Net.register.Register_DoubleLongRegister register) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
      private static long ReadUpdates(IIndexReference index, SchemaRead schemaRead, Neo4Net.Register.Register_DoubleLongRegister register)
      {
         schemaRead.IndexUpdatesAndSize(index, register);
         return register.ReadFirst();
      }
   }
}