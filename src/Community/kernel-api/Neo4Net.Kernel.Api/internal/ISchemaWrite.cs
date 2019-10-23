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
   using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;
   using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor;
   using RelationTypeSchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.RelationTypeSchemaDescriptor;
   using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;

   /// <summary>
   /// Surface for creating and dropping indexes and constraints.
   /// </summary>
   public interface ISchemaWrite
   {
      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor);

      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <param name="name"> name of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor, java.util.Optional<String> name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor, Optional<string> name);

      /// <summary>
      /// Create index from schema descriptor
      /// </summary>
      /// <param name="descriptor"> description of the index </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      /// <param name="name"> name of the index </param>
      /// <returns> the newly created index </returns>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: IndexReference indexCreate(org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor, String provider, java.util.Optional<String> name) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      IIndexReference IndexCreate(SchemaDescriptor descriptor, string provider, Optional<string> name);

      /// <summary>
      /// Drop the given index
      /// </summary>
      /// <param name="index"> the index to drop </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void indexDrop(IndexReference index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      void IndexDrop(IIndexReference index);

      /// <summary>
      /// Create unique property constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor UniquePropertyConstraintCreate(SchemaDescriptor descriptor);

      /// <summary>
      /// Create unique property constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor uniquePropertyConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor descriptor, String provider) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor UniquePropertyConstraintCreate(SchemaDescriptor descriptor, string provider);

      /// <summary>
      /// Create node key constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor NodeKeyConstraintCreate(LabelSchemaDescriptor descriptor);

      /// <summary>
      /// Create node key constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      /// <param name="provider"> name of the desired index provider implementation </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor nodeKeyConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor descriptor, String provider) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor NodeKeyConstraintCreate(LabelSchemaDescriptor descriptor, string provider);

      /// <summary>
      /// Create node property existence constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor nodePropertyExistenceConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.LabelSchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor NodePropertyExistenceConstraintCreate(LabelSchemaDescriptor descriptor);

      /// <summary>
      /// Create relationship property existence constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor relationshipPropertyExistenceConstraintCreate(org.Neo4Net.Kernel.Api.Internal.schema.RelationTypeSchemaDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      ConstraintDescriptor RelationshipPropertyExistenceConstraintCreate(RelationTypeSchemaDescriptor descriptor);

      /// <summary>
      /// Drop constraint
      /// </summary>
      /// <param name="descriptor"> description of the constraint </param>
      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: void constraintDrop(org.Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor descriptor) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
      void ConstraintDrop(ConstraintDescriptor descriptor);
   }
}