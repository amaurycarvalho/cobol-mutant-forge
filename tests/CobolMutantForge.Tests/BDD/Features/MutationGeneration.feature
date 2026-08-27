Feature: Mutation Generation

  Scenario: Logical operator mutant
    Given a program containing "IF A > B AND C = D"
    When I generate mutations under the medium profile
    Then a mutant replacing "AND" with "OR" is produced

  Scenario: Arithmetic operator mutant
    Given a program containing "COMPUTE TOTAL = AMOUNT + TAX"
    When I generate mutations under the medium profile
    Then a mutant replacing "+" with "-" is produced
