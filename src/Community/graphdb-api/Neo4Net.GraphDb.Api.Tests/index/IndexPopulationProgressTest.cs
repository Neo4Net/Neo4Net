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

using Xunit;

namespace Neo4Net.GraphDb.Index
{
   public class IndexPopulationProgressTest
   {
      [Fact]
      public void TestNone()
      {
         Assert.Equal(0, IndexPopulationProgress.None.CompletedPercentage) // $!!$ tac, 0.01);
        }

      [Fact]
      public void TestDone()
      {
         Assert.Equal(100, IndexPopulationProgress.Done.CompletedPercentage); // $!!$ tac, 0.01);
      }

      [Fact]
      public void TestNegativeCompleted()
      {
         Assert.Throws<System.ArgumentException>(() => new IndexPopulationProgress(-1, 1));
      }

      [Fact]
      public void TestNegativeTotal()
      {
         Assert.Throws<System.ArgumentException>(() => new IndexPopulationProgress(0, -1));
      }

      [Fact]
      public void TestAllZero()
      {
         IndexPopulationProgress progress = new IndexPopulationProgress(0, 0);
         Assert.Equal(0, progress.CompletedCount);
         Assert.Equal(0, progress.TotalCount);
         Assert.Equal(0, progress.CompletedPercentage); //$!!$ tac , 0.01);
      }

      [Fact]
      public void TestCompletedZero()
      {
         IndexPopulationProgress progress = new IndexPopulationProgress(0, 1);
         Assert.Equal(1, progress.TotalCount);
         Assert.Equal(0, progress.CompletedCount);
         Assert.Equal(0, progress.CompletedPercentage) //$!!$ tac, 0.01);
        }

      [Fact]
      public void TestCompletedGreaterThanTotal()
      {
         Assert.Throws<System.ArgumentException>(() => new IndexPopulationProgress(2, 1));
      }

      [Fact]
      public void TestGetCompletedPercentage()
      {
         IndexPopulationProgress progress = new IndexPopulationProgress(1, 2);
         Assert.Equal(50.0f, progress.CompletedPercentage); //$!!$ tac, 0.01f);
      }

      [Fact]
      public void TestGetCompleted()
      {
         IndexPopulationProgress progress = new IndexPopulationProgress(1, 2);
         Assert.Equal(1L, progress.CompletedCount);
      }

      [Fact]
      public void TestGetTotal()
      {
         IndexPopulationProgress progress = new IndexPopulationProgress(1, 2);
         Assert.Equal(2L, progress.TotalCount);
      }
   }
}