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

namespace Neo4Net.Kernel.Api.Internal
{
   /// <summary>
   /// Methods for acquiring and releasing locks.
   /// </summary>
   public interface ILocks
   {
      void AcquireExclusiveNodeLock(params long[] ids);

      void AcquireExclusiveRelationshipLock(params long[] ids);

      void AcquireExclusiveExplicitIndexLock(params long[] ids);

      void AcquireExclusiveLabelLock(params long[] ids);

      void ReleaseExclusiveNodeLock(params long[] ids);

      void ReleaseExclusiveRelationshipLock(params long[] ids);

      void ReleaseExclusiveExplicitIndexLock(params long[] ids);

      void ReleaseExclusiveLabelLock(params long[] ids);

      void AcquireSharedNodeLock(params long[] ids);

      void AcquireSharedRelationshipLock(params long[] ids);

      void AcquireSharedExplicitIndexLock(params long[] ids);

      void AcquireSharedLabelLock(params long[] ids);

      void ReleaseSharedNodeLock(params long[] ids);

      void ReleaseSharedRelationshipLock(params long[] ids);

      void ReleaseSharedExplicitIndexLock(params long[] ids);

      void ReleaseSharedLabelLock(params long[] ids);
   }
}