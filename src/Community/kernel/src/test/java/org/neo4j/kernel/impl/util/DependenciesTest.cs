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
namespace Neo4Net.Kernel.impl.util
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class DependenciesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeWhenResolveWithTypeThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeWhenResolveWithTypeThenInstanceReturned()
		 {
			  // Given
			  Dependencies dependencies = new Dependencies();

			  string foo = "foo";
			  dependencies.SatisfyDependency( foo );

			  // When
			  string instance = dependencies.ResolveDependency( typeof( string ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeWhenResolveWithSuperTypeThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeWhenResolveWithSuperTypeThenInstanceReturned()
		 {
			  // Given
			  Dependencies dependencies = new Dependencies();

			  System.Collections.ObjectModel.Collection foo = new List<object>();
			  dependencies.SatisfyDependency( foo );

			  // When
			  System.Collections.ObjectModel.Collection instance = dependencies.ResolveDependency( typeof( System.Collections.ObjectModel.Collection ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeWhenResolveWithInterfaceThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeWhenResolveWithInterfaceThenInstanceReturned()
		 {
			  // Given
			  Dependencies dependencies = new Dependencies();

			  System.Collections.IList foo = new List<object>();
			  dependencies.SatisfyDependency( foo );

			  // When
			  System.Collections.IList instance = dependencies.ResolveDependency( typeof( System.Collections.IList ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeWhenResolveWithSubInterfaceThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeWhenResolveWithSubInterfaceThenInstanceReturned()
		 {
			  // Given
			  Dependencies dependencies = new Dependencies();

			  System.Collections.ICollection foo = new List<object>();
			  dependencies.SatisfyDependency( foo );

			  // When
			  System.Collections.ICollection instance = dependencies.ResolveDependency( typeof( System.Collections.ICollection ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeInParentWhenResolveWithTypeInEmptyDependenciesThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeInParentWhenResolveWithTypeInEmptyDependenciesThenInstanceReturned()
		 {
			  // Given
			  Dependencies parent = new Dependencies();
			  Dependencies dependencies = new Dependencies( parent );

			  System.Collections.ICollection foo = new List<object>();
			  dependencies.SatisfyDependency( foo );

			  // When
			  System.Collections.ICollection instance = dependencies.ResolveDependency( typeof( System.Collections.ICollection ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenSatisfiedTypeInParentAndDependenciesWhenResolveWithTypeInDependenciesThenInstanceReturned()
		 public virtual void GivenSatisfiedTypeInParentAndDependenciesWhenResolveWithTypeInDependenciesThenInstanceReturned()
		 {
			  // Given
			  Dependencies parent = new Dependencies();
			  Dependencies dependencies = new Dependencies( parent );

			  System.Collections.ICollection foo = new List<object>();
			  dependencies.SatisfyDependency( foo );
			  parent.SatisfyDependency( new List<object>() );

			  // When
			  System.Collections.ICollection instance = dependencies.ResolveDependency( typeof( System.Collections.ICollection ) );

			  // Then
			  assertThat( instance, equalTo( foo ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenEmptyDependenciesWhenResolveWithTypeThenException()
		 public virtual void GivenEmptyDependenciesWhenResolveWithTypeThenException()
		 {
			  // Given
			  Dependencies dependencies = new Dependencies();

			  // When
			  try
			  {
					dependencies.ResolveDependency( typeof( System.Collections.ICollection ) );
					fail();
			  }
			  catch ( UnsatisfiedDependencyException )
			  {
					// Then
			  }
		 }
	}

}