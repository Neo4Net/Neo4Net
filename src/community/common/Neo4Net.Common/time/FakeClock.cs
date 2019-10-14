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

namespace Neo4Net.Time
{
   /// <summary>
   /// A <seealso cref="java.time.Clock"/> that is manually controlled.
   /// The implementation is thread safe.
   /// </summary>
   public class FakeClock : SystemNanoClock
   {
      private AtomicLong _nanoTime = new AtomicLong();

      public FakeClock()
      {
      }

      public FakeClock(long initialTime, TimeUnit unit)
      {
         Forward(initialTime, unit);
      }

      public override ZoneId Zone
      {
         get
         {
            return ZoneOffset.UTC;
         }
      }

      public override Clock WithZone(ZoneId zone)
      {
         return new WithZone(this, zone);
      }

      public override Instant Instant()
      {
         return Instant.ofEpochMilli(TimeUnit.NANOSECONDS.toMillis(_nanoTime.get()));
      }

      public override long Nanos()
      {
         return _nanoTime.get();
      }

      public override long Millis()
      {
         return TimeUnit.NANOSECONDS.toMillis(_nanoTime.get());
      }

      public virtual FakeClock Forward(Duration delta)
      {
         return Forward(delta.toNanos(), TimeUnit.NANOSECONDS);
      }

      public virtual FakeClock Forward(long delta, TimeUnit unit)
      {
         _nanoTime.addAndGet(unit.toNanos(delta));
         return this;
      }

      private class WithZone : Clock
      {
         private readonly FakeClock _outerInstance;

         //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
         internal readonly ZoneId ZoneConflict;

         internal WithZone(FakeClock outerInstance, ZoneId zone)
         {
            this._outerInstance = outerInstance;
            this.ZoneConflict = zone;
         }

         public override ZoneId Zone
         {
            get
            {
               return ZoneConflict;
            }
         }

         //JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
         public override Clock WithZoneConflict(ZoneId zone)
         {
            return new WithZone(_outerInstance, zone);
         }

         public override long Millis()
         {
            return _outerInstance.millis();
         }

         public override Instant Instant()
         {
            return _outerInstance.instant();
         }
      }
   }
}