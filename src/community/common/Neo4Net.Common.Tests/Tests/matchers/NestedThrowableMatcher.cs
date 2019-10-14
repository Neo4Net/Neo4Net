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

namespace Neo4Net.Test.matchers
{
   using Description = org.hamcrest.Description;

   public class NestedThrowableMatcher : TypeSafeMatcher<Exception>
   {
      private readonly Type _expectedType;

      public NestedThrowableMatcher(Type expectedType)
      {
         this._expectedType = expectedType;
      }

      public override void DescribeTo(Description description)
      {
         description.appendText("expect ").appendValue(_expectedType).appendText(" to be exception cause.");
      }

      protected internal override bool MatchesSafely(Exception item)
      {
         Exception currentThrowable = item;
         do
         {
            if (_expectedType.IsInstanceOfType(currentThrowable))
            {
               return true;
            }
            currentThrowable = currentThrowable.InnerException;
         } while (currentThrowable != null);
         return false;
      }
   }
}